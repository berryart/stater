using System.Collections.Generic;
using System.Threading.Tasks;


namespace Stater {
    public interface ISubscrier {
        void Render(Action action);
    }

    public class Store {
        List<ISubscrier> subscriers = new List<ISubscrier>();
        public State State {
            get; protected set;
        }
        public State PrevState {
            get; protected set;
        }

        public Store() {
            State = new State();
        }

        public Store(string state) {
            State = State.Parse(state);
        }

        public Store(State state) {
            State = state;
        }

        public void Subscribe(ISubscrier subscrier) {
            if (subscriers.Contains(subscrier)) {
                System.Console.WriteLine($"Subscriber {subscrier} is already subscribed to the store! Skipping it.");
                return;
            }
            subscriers.Add(subscrier);
        }

        public void Unsubscribe(ISubscrier subscrier) {
            subscriers.Remove(subscrier);
        }

        public void Dispatch(Action action, bool checkValidity = true) {
            if (checkValidity && !action.IsValid)
                throw new System.Exception("Action is not valid!");

            PrevState = State;
            State = action.Apply(State.Clone());
            
            foreach (var sub in subscriers)
                sub.Render(action);
        }

        public async Task DispatchAsync(AsyncAction action) {
            if (!action.IsValid)
                throw new System.Exception("Action is not valid!");

            await action.CallAsync();
            Dispatch(action, checkValidity: false);
        }

        public void Rollback() {
            State = PrevState;
        }
    }
}