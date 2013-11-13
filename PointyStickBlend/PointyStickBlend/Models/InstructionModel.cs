using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointyStickBlend.Models
{
    public class InstructionModel : ObservableCollection<Instruction>
    {
        public InstructionModel() : base() { }
        public InstructionModel(List<Instruction> list) : base(list) { }
        public InstructionModel(IEnumerable<Instruction> collection) : base(collection) { }
    }   
}
