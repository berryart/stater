using NUnit.Framework;
using System;
using Stater;
using System.Collections.Generic;

namespace test {
    public class Tree : IStatable {
        public string key;
        public int root_number, leaf_number;
        
        public void SetState(string key, State state) {
            Console.WriteLine(state.ToJson());
            this.key = key;
            root_number = state["root_number"];
            leaf_number = state["branch"]["leaf_number"];
        }

        public State GetState() {
            return null;
        }
    }
    public class StateTest {
        [Test]
        public void state() {
            var root = new State();
            root["root_number"] = 5;

            var branch = new State();
            branch["leaf_number"] = 1;

            root["branch"] = branch;

            var res = root.To<Tree>();
            Assert.IsInstanceOf(typeof(Tree), res);
            Assert.AreEqual(5, res.root_number);
            Assert.AreEqual(1, res.leaf_number);
        }

        [Test]
        public void clone() {
            var root = new State();
            root["root_number"] = 5;

            var branch = new State();
            branch["leaf_number"] = 1;

            root["branch"] = branch;

            var clone = root.Clone();

            Assert.AreEqual(5, clone["root_number"].ToInt());
            Assert.AreEqual(1, clone["branch"]["leaf_number"].ToInt());

            clone["root_number"] = 6;
            Assert.AreEqual(6, clone["root_number"].ToInt());
            Assert.AreEqual(5, root["root_number"].ToInt());
        }

        static bool TestPredicate(State state) {
            return state["predicate"].ToBool();
        }

        [Test]
        public void predicate_clone() {
            var str = @"{
                'state1': {
                    'predicate': true
                },
                'state2': {
                    'predicate': false
                },
                'state3': {
                    'predicate': true
                }
            }";
            var state = State.Parse(str);
            Assert.IsTrue(state["state1"]["predicate"].ToBool());
            Assert.IsFalse(state["state2"]["predicate"].ToBool());

            var predicate = new System.Predicate<State>(TestPredicate);
            var clone = state.Clone(predicate);

            Assert.IsFalse(clone.HasKey("state2"));
            Assert.IsTrue(clone["state1"]["predicate"].ToBool());
            Assert.IsTrue(clone["state3"]["predicate"].ToBool());
        }

        [Test]
        public void parse() {
            var str = @"{
                'root_value': true,
                'root_state': {
                    'sub_value': 6,
                    'sub_string': 'string:with:colon',
                }
            }".Replace("'", "\"");

            var state = State.Parse(str);

            Assert.AreEqual(true, state["root_value"].ToBool());
            Assert.AreEqual(6, state["root_state"]["sub_value"].ToInt());
            Assert.AreEqual("string:with:colon", state["root_state"]["sub_string"].ToString());
        }

        [Test]
        public void from_dict() {
            var dict = new Dictionary<string, object>() {
                {"key0", "stringValue"},
                {"key1", 3},
                {"key5", new Dictionary<string, object>() {
                    {"key2", 4.5},
                    {"key3", true},
                }},
            };

            var state = State.FromDict(dict);

            string str = state["key0"];
            Assert.AreEqual("stringValue", str);
            
            int integer = state["key1"];
            Assert.AreEqual(3, integer);

            Assert.IsTrue(state["key5"].IsCompound);

            float flt = state["key5"]["key2"];
            Assert.AreEqual(4.5f, flt);

            bool bl = state["key5"]["key3"];
            Assert.IsTrue(bl);
        }

        [Test]
        public void to_json() {
            var str = @"{
                'bool': true,
                'string': 'strig value',
                'state': {
                    'int': 6
                }
            }".Replace("'", "\"");
            var state = State.Parse(str);

            var res = state.ToJson();

            var expected = "{'bool': True,'string': 'strig value','state': {'int': 6,},}".Replace("'", "\"");
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void operators() {
            // Implicit coversion
            State s = "5";
            State i = 5;
            State j = 5;
            State t = true;

            int ii = new State(5);
            Assert.AreEqual(5, ii);

            string ss = new State("ss");
            Assert.AreEqual("ss", ss);

            // +
            var a = new State(2);
            var b = new State(3);
            // var res = a + b;
            // Assert.AreEqual(5, res.ToInt());

            // == & !=
            a = new State(5);
            b = new State("string");
            Assert.IsTrue(a == 5);
            Assert.IsFalse(a == 6);
            Assert.IsTrue(b == "string");
            Assert.IsFalse(b == "str");

            // >= && <=
            a = new State(1);
            Assert.IsTrue(a < 2);
            Assert.IsTrue(a <= 1);
        }

        // [Test]
        // public void clone_time() {
        //     var path = "/Users/artem.yagodin/Desktop/Desktop/toti/State.json";
        //     var stateText = System.IO.File.ReadAllText(path);

        //     var state = State.Parse(stateText);

        //     var watch = System.Diagnostics.Stopwatch.StartNew();

        //     var clone = state.Clone();
        //     Console.Write(clone.ToJson());

        //     watch.Stop();
        //     Console.WriteLine($"Clone time: {watch.ElapsedMilliseconds} ms");

        //     Assert.IsFalse(true);
        // }
    }
}