using System.Threading.Tasks;


namespace Stater {
    public abstract class Action {
        public virtual bool IsValid {
            get { return false; }
        }

        public virtual State Apply(State state) {
            throw new System.Exception("Unimplemented action!");
        }
    }

    public abstract class AsyncAction : Action {
        public virtual async Task CallAsync() {
            await Task.Delay(0);
            throw new System.Exception("Unimplemented action!");
        }
    }
}