using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards {

    public class CardPackageData {

        public int PackageId { get; set; }
        public List<string> CardList { get; set; } = new List<string>();

        public CardPackageData(int packageId, List<string> includedCards) {

            PackageId = packageId;
            CardList = includedCards ?? new List<string>();
        }

        public CardPackageData() {
            
        }
    }
}
