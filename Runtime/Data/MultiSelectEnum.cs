using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    public class MultiSelectEnum : PropertyAttribute {

        public int skip = 1;

        public MultiSelectEnum(int skipOptions = 1) {
            this.skip = skipOptions;
        }

    }

}
