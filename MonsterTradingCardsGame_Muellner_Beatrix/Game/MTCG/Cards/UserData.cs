using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards {

    public class UserData {

        public int PackageId { get; set; }
        public string OwnerAccount { get; set; }
        public string SecurePassword { get; set; }
        public int Coins { get; set; }
        public List<string> DeckList { get; set; }
        public int VictoryCount { get; set; }
        public int LossCount { get; set; }
        public int RatingPoints { get; set; }
        public int MatchesPlayed { get; set; }
        public string Image { get; set; }
        public string PublicName { get; set; }
        public string Biography { get; set; }

        public UserData() {

            SecurePassword = "";
        }

        public UserData(int PackageId, string OwnerAccount, string securePassword, int coins, List<string> deckList, int victoryCount, int lossCount, int ratingPoints, int matchesPlayed) {

            PackageId = PackageId;
            OwnerAccount = OwnerAccount;
            SetPassword(securePassword);
            Coins = coins;
            DeckList = deckList ?? new List<string>();
            VictoryCount = victoryCount;
            LossCount = lossCount;
            RatingPoints = ratingPoints;
            MatchesPlayed = matchesPlayed;
        }

        public UserData(int PackageId, string OwnerAccount, int coins, List<string> deckList, int victoryCount, int lossCount, int ratingPoints, int matchesPlayed, string publicName, string biography, string image)
            : this(PackageId, OwnerAccount, "", coins, deckList, victoryCount, lossCount, ratingPoints, matchesPlayed) {

            PublicName = publicName;
            Biography = biography;
            Image = image;
        }

        public void SetPassword(string newPassword) {

            SecurePassword = newPassword;
        }

        public bool IsValid() {

            return !string.IsNullOrEmpty(OwnerAccount) && !string.IsNullOrEmpty(SecurePassword);
        }



    }
}
