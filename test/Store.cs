using NUnit.Framework;
using Stater;


namespace test {
    public class StoreTest {
        class SetKeyAction : Action {
            public override bool IsValid => true;

            public override State Apply(State state) {
                state["key"] = new State(5);
                return state;
            }
        }

        [Test]
        public void rollback() {
            var store = new Store(@"{
                'key': 1
            }");
            Assert.AreEqual(1, store.State["key"].ToInt());
            
            store.Dispatch(new SetKeyAction());
            Assert.AreEqual(5, store.State["key"].ToInt());
            
            store.Rollback();
            Assert.AreEqual(1, store.State["key"].ToInt());
        }
    }
}