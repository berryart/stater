using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;


namespace Stater {
    public interface IStatable {
        void SetState(string key, State state);
        State GetState();
    }

    public class State {
        public bool IsCompound {
            get { return Dict != null; }
        }

        object value;
        public Dictionary<string, State> Dict;

        public State this [string key] {
            get {
                if (key == null)
                    throw new Exception("Unable to get key: given key is null!");
                if (!IsCompound)
                    throw new Exception($"Unable to get value: state is not compound!");
                State state;
                if (Dict.TryGetValue(key, out state))
                    return state;
                throw new Exception($"Unable to get value for key {key}!");
            }
            set {
                if (key == null)
                    throw new Exception("Unable to add key: given value is null!");
                if (!IsCompound)
                    throw new Exception("Unable to add key to non-compound state!");
                if (Dict.ContainsKey(key))
                    Dict[key] = value;
                else
                    Dict.Add(key, value);
            }
        }

        public State(object value) {
            this.value = value;
        }

        public State() {
            Dict = new Dictionary<string, State>();
        }

        public static implicit operator State(string value) {
            return new State(value);
        }
        public static implicit operator State(int value) {
            return new State(value);
        }
        public static implicit operator State(bool value) {
            return new State(value);
        }

        public static implicit operator string(State value) {
            if (value.IsCompound)
                throw new Exception($"Unablet to assign state to string: state is compound!");
            return value.ToString();
        }
        public static implicit operator int(State value) {
            return value.ToInt();
        }
        public static implicit operator float(State value) {
            return float.Parse(value);
        }
        public static implicit operator bool(State value) {
            return value.ToBool();
        }

        public bool HasKey(string key) {
            if (!IsCompound)
                throw new Exception("State is not compound!");
                
            return Dict.ContainsKey(key);
        }

        public State TryGet(string key) {
            if (!IsCompound)
                throw new Exception($"Unable to try get value for key {key}: state is nor compound!");

            State state = null;
            Dict.TryGetValue(key, out state);
            return state;
        }

        public T To<T>() where T: IStatable, new() {
            // Can be used for extra types different from asic types int, bool, string
            if (!IsCompound)
                throw new Exception("Unbale to convert state: state is not compound!");
            var res = new T();
            res.SetState(null, this);
            return res;
        }

        public int ToInt() {
            return Convert.ToInt32(value);
        }

        public string StringOrDef(string key) {
            if (!Dict.ContainsKey(key))
                return null;
            return Dict[key].ToString();
        }

        public int IntOrDef(string key) {
            if (!Dict.ContainsKey(key))
                return 0;
            return Dict[key].ToInt();
        }

        public bool BoolOrDef(string key) {
            if (!Dict.ContainsKey(key))
                return false;
            return Dict[key].ToBool();
        }

        public override string ToString() {
            return Convert.ToString(value);
        }

        public bool StartsWith(string v) {
            return ToString().StartsWith(v);
        }

        public bool ToBool() {
            return Convert.ToBoolean(value);
        }

        public List<T> ToList<T>() where T : IStatable, new() {
            var list = new List<T>();

            foreach (var pair in Dict) {
                var t = new T();
                t.SetState(pair.Key, pair.Value);
                list.Add(t);
            }

            return list;
        }

        public State Clone() {
            if (!IsCompound)
                return new State(value);
            var clone = new State();
            foreach (var pair in Dict)
                clone[pair.Key] = pair.Value.Clone();
            return clone;
        }

        public State Clone(Predicate<State> p) {
            if (!IsCompound)
                return new State(value);
            var clone = new State();
            foreach (var pair in Dict)
                if (p.Invoke(pair.Value))
                    clone[pair.Key] = pair.Value.Clone();
            return clone;
        }

        public static State FromDict(Dictionary<string, object> dict) {
            if (dict == null)
                throw new Exception($"Unable to create state from dictionary: given dict is null!");

            var state = new State();
            foreach (var pair in dict) {
                if (pair.Value is Dictionary<string, object>) {
                    state[pair.Key] = FromDict(pair.Value as Dictionary<string, object>);
                }
                else {
                    state[pair.Key] = new State(pair.Value);
                }
            }

            return state;
        }

        public static State Parse(string str) {
            // Cleanup the string (remove all spaces except those that in quotes)
            string cleanStr = Regex.Replace(str, "\\s+(?=([^\"]*\"[^\"]*\")*[^\"]*$)", ""); // Regex.Replace(str, @"\s+", "");
            return FromJson(cleanStr);
        }

        static State FromJson(string str) {
            // Make a result dict
            var res = new State();

            // Master string contains top level nodes
            string master = "";

            // Objects is a list with nested objects for each key that has an object
            List<(int, int)> objects = new List<(int, int)>();

            // Bracker tracks openning and closing brackets
            int bracker = 0;

            // obj tuple holds object string start position and length
            // used to extract a substring of a nested object
            var obj = (-1, -1);

            // Run throug a string and make a master string and objects' indexes
            for (int i = 1; i < str.Length - 1; i++) {
                if (str[i] == '{') {
                    bracker += 1;
                    if (bracker == 1)
                        obj.Item1 = i;
                } 
                else if (str[i] == '}') {
                    bracker -= 1;
                    if (bracker == 0) {
                        obj.Item2 = i - obj.Item1 + 1;
                        objects.Add((obj.Item1, obj.Item2));
                    }
                } 
                else if (bracker == 0)
                    master += str[i];
            }

            // Check if master string is valid
            if (master.Length == 0) {
                // Console.WriteLine("Emty master string received!");
                return res;
            }

            // Split master string into pairs
            string[] pairs = master.Split(',');

            // Foreach pair make a dict entry and call recursively making a dict if object found
            foreach (string pair in pairs) {
                // Skip empty pairs left after last comma in the json nodes
                if (pair.Length == 0)
                    continue;

                string[] keyValue = pair.Split(new char[] {':'}, 2);
                if (keyValue.Length != 2)
                    throw new Exception($"Unable to parse json: illegal key-value pair '{pair}'");

                string cleanKey = keyValue[0].Substring(1, keyValue[0].Length - 2);
                if (keyValue[1] != "") {
                    string cleanValue = keyValue[1];
                    if (keyValue[1].StartsWith("\"")) {
                        cleanValue = keyValue[1].Substring(1, keyValue[1].Length - 2);
                    }
                    res[cleanKey] = new State(cleanValue);
                } 
                else {
                    res[cleanKey] = FromJson(str.Substring(objects[0].Item1, objects[0].Item2));
                    objects.RemoveAt(0);
                }
            }

            return res;
        }

        public string ToJson() {
            if (!IsCompound) {
                var str = value != null ? value.ToString() : "null";

                // Number
                int intval;
                var res = int.TryParse(str, out intval);
                if (res)
                    return intval.ToString();
                
                // Boolean
                bool boolval;
                res = bool.TryParse(str, out boolval);
                if (res)
                    return boolval.ToString();

                return "\"" + str + "\"";
            }

            var result = "{";
            foreach (var kv in Dict) {
                var val = kv.Value != null ? kv.Value.ToJson() : "null";
                result += "\"" + kv.Key + "\": " + kv.Value.ToJson() + ",";
            }
            result += "}";
            
            return result;
        }
    }
}