using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards {

    public class CardData {

        public string PackageId { get; set; }
        public string CardName { get; set; }
        public double AttackPower { get; set; }
        public string ElementAttribute { get; set; }
        public string CardType { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerAccount { get; set; }

        public CardData(string packageId, string cardName, double attackPower, string elementAttribute, string cardType, int? ownerId = null, string ownerAccount = null) {

            PackageId = packageId;
            CardName = cardName;
            AttackPower = attackPower;
            ElementAttribute = elementAttribute;
            CardType = cardType;
            OwnerId = ownerId;
            OwnerAccount = ownerAccount;
        }

        public CardData() {

        }

        public bool IsValid() {

            return !string.IsNullOrEmpty(CardName) && AttackPower > 0 && !string.IsNullOrEmpty(ElementAttribute) && !string.IsNullOrEmpty(CardType);
        }

        public void AssignToOwner(int ownerId, string ownerAccount) {

            OwnerId = ownerId;
            OwnerAccount = ownerAccount;
        }
    }
}
