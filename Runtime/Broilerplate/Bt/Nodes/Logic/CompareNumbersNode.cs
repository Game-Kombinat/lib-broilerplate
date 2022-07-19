using Broilerplate.Bt.Nodes.Ports;
using GameKombinat.ControlFlow.Bt;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Logic {
    [NodeTint(NodeColors.Blue)]
    [CreateNodeMenu("Logic/Number Compare")]
    public class CompareNumbersNode : BaseNode {

        [Input]
        public NbtFloatPort floatA;
        [Input]
        public NbtFloatPort floatB;
        [Input]
        public NbtNumberPort numberC;
        [Input]
        public NbtIntPort integerD;
        [Input]
        public NbtStringPort stringE;
        [Input]
        public NbtBooleanPort boolF;
        
        protected override void InternalSpawn() {
            
        }

        protected override TaskStatus InternalTick() {
            var valueA = GetInputValue<NbtTag>(nameof(floatA));
            var valueB = GetInputValue<NbtTag>(nameof(floatB));
            var valueC = GetInputValue<NbtTag>(nameof(numberC));
            var valueD = GetInputValue<NbtTag>(nameof(integerD));
            var valueE = GetInputValue<NbtTag>(nameof(stringE));
            var valueF = GetInputValue<NbtTag>(nameof(boolF));
            
            Debug.Log("Addition is " + (valueA.FloatValue + valueB.FloatValue));
            Debug.Log("Number Port:  " + (valueC.FloatValue));
            Debug.Log("Int Port:  " + (valueD.IntValue));
            Debug.Log("String Port:  " + (valueE.StringValue));
            Debug.Log("Boolean Port:  " + (valueF.ByteValue));
            return TaskStatus.Success;
        }

        protected override void InternalTerminate() {
            
        }
    }
}