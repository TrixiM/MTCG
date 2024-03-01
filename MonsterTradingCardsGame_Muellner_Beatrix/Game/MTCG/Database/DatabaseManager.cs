using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using Npgsql;
using MTCG.Cards;

namespace MTCG.Database {

    public class DB_Functions {

        public readonly string connectionString = "Host=localhost;OwnerAccount=Beatrix;SecurePassword=Trixi;Database=mtcgdb;Port=5433";

        public NpgsqlCommand PrepareRegisterUserCommand(NpgsqlConnection dbConnection, UserData accountDetails) {

            string sqlQuery = @"
                INSERT INTO users 
                (OwnerAccount, SecurePassword, coins, DeckList, VictoryCount, LossCount, RatingPoints, MatchesPlayed, PublicName, Biography, image) 
                VALUES 
                (@OwnerAccount, @PasswordHash, @InitialCoins, @DeckList, @WinCount, @LossCount, @EloRating, @MatchesPlayed, '', '', '')";

            var command = new NpgsqlCommand(sqlQuery, dbConnection);
            command.Parameters.AddWithValue("@OwnerAccount", accountDetails.OwnerAccount);
            command.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(accountDetails.SecurePassword));
            command.Parameters.AddWithValue("@InitialCoins", 20);
            command.Parameters.AddWithValue("@DeckList", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, new string[] { });
            command.Parameters.AddWithValue("@WinCount", 0);
            command.Parameters.AddWithValue("@LossCount", 0);
            command.Parameters.AddWithValue("@EloRating", 1000);
            command.Parameters.AddWithValue("@MatchesPlayed", 0);

            return command;
        }

        public async Task<bool> RegisterUser(UserData accountDetails) {

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            var command = PrepareRegisterUserCommand(databaseConnection, accountDetails);
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows == 1;
        }

        public NpgsqlCommand PrepareUpdateStatsCommand(NpgsqlConnection dbConnection, UserData user) {

            string sqlUpdate = @"
                UPDATE users 
                SET VictoryCount = @WinCount, LossCount = @LossCount, RatingPoints = @EloRating, MatchesPlayed = @MatchesPlayed 
                WHERE OwnerAccount = @OwnerAccount";

            var command = new NpgsqlCommand(sqlUpdate, dbConnection);
            command.Parameters.AddWithValue("@WinCount", user.VictoryCount);
            command.Parameters.AddWithValue("@LossCount", user.LossCount);
            command.Parameters.AddWithValue("@EloRating", user.RatingPoints);
            command.Parameters.AddWithValue("@MatchesPlayed", user.MatchesPlayed);
            command.Parameters.AddWithValue("@OwnerAccount", user.OwnerAccount);

            return command;
        }

        public async Task<bool> UpdateUserStats(UserData user) {

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            var command = PrepareUpdateStatsCommand(databaseConnection, user);
            int rowsUpdated = await command.ExecuteNonQueryAsync();

            return rowsUpdated == 1;
        }

        public NpgsqlCommand PrepareUpdateProfileCommand(NpgsqlConnection dbConnection, UserData oldUser, UserData newUser) {

            string commandText = @"
                UPDATE users 
                SET PublicName = @PublicName, Biography = @Biography, image = @Image 
                WHERE OwnerAccount = @OwnerAccount";

            var command = new NpgsqlCommand(commandText, dbConnection);
            command.Parameters.AddWithValue("@OwnerAccount", oldUser.OwnerAccount);
            command.Parameters.AddWithValue("@PublicName", newUser.PublicName);
            command.Parameters.AddWithValue("@Biography", newUser.Biography);
            command.Parameters.AddWithValue("@Image", newUser.Image);

            return command;
        }

        public async Task<bool> UpdateUserProfile(UserData oldUser, UserData newUser) {

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            var command = PrepareUpdateProfileCommand(databaseConnection, oldUser, newUser);
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows == 1;
        }

        public async Task<UserData> RetrieveUserBySession(string token) {

            string query = @"
                SELECT PackageId  , OwnerAccount, coins, DeckList, VictoryCount, LossCount, RatingPoints, MatchesPlayed, PublicName, Biography, image 
                FROM users 
                WHERE token = @Token";

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new NpgsqlCommand(query, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@Token", token);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            if (await dataReader.ReadAsync()) {

                return new UserData(
                    dataReader.GetInt32(0),
                    dataReader.GetString(1),
                    dataReader.GetInt32(2),
                    dataReader.GetFieldValue<List<string>>(3),
                    dataReader.GetInt32(4),
                    dataReader.GetInt32(5),
                    dataReader.GetInt32(6),
                    dataReader.GetInt32(7),
                    dataReader.GetString(8),
                    dataReader.GetString(9),
                    dataReader.GetString(10)
                );
            }
            return null;
        }


        public async Task<UserData> FetchUserDetails(string sessionToken) {

            string query = @"
                SELECT PackageId  , OwnerAccount, coins, DeckList, VictoryCount, LossCount, RatingPoints, MatchesPlayed, PublicName, Biography, image 
                FROM users 
                WHERE token = @SessionToken";

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new NpgsqlCommand(query, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@SessionToken", sessionToken);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            if (await dataReader.ReadAsync()) {

                return new UserData(
                    dataReader.GetInt32(0),  
                    dataReader.GetString(1),
                    dataReader.GetInt32(2),
                    dataReader.GetFieldValue<List<string>>(3),
                    dataReader.GetInt32(4),
                    dataReader.GetInt32(5),
                    dataReader.GetInt32(6),
                    dataReader.GetInt32(7),
                    dataReader.GetString(8),
                    dataReader.GetString(9),
                    dataReader.GetString(10)
                );
            }
            return null;
        }

        public async Task AuthenticateUser(string token, UserData user) {

            string updateTokenQuery = @"
                UPDATE users 
                SET token = @NewToken 
                WHERE OwnerAccount = @UserIdentifier";

            await using var dbConn = new NpgsqlConnection(connectionString);
            await dbConn.OpenAsync();

            var updateTokenCmd = dbConn.CreateCommand();
            updateTokenCmd.CommandText = updateTokenQuery;

            updateTokenCmd.Parameters.Add(new NpgsqlParameter("@NewToken", token));
            updateTokenCmd.Parameters.Add(new NpgsqlParameter("@UserIdentifier", user.OwnerAccount));

            await updateTokenCmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> PurchaseCardPackage(UserData user) {

            await using var dbConn = new NpgsqlConnection(connectionString);
            await dbConn.OpenAsync();

            using var transaction = await dbConn.BeginTransactionAsync();

            string assignCardsSql = @"
                WITH assigned_package AS (
                    DELETE FROM packages 
                    WHERE PackageId   = (SELECT PackageId   FROM packages ORDER BY PackageId   FOR UPDATE SKIP LOCKED LIMIT 1)
                    RETURNING CardList
                )
                UPDATE CardList 
                SET OwnerAccount = @OwnerAccount 
                FROM assigned_package 
                WHERE CardList.PackageId   = ANY(assigned_package.CardList)";

            int cardsAssigned = await ExecuteSql(dbConn, assignCardsSql, user.OwnerAccount);

            if (cardsAssigned == 0) {

                await transaction.RollbackAsync();
                return false;
            }

            bool coinsUpdated = await UpdateUserCoins(dbConn, user.OwnerAccount, user.Coins - 5);

            if (!coinsUpdated) {

                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }

        public async Task<int> ExecuteSql(NpgsqlConnection dbConn, string sql, string OwnerAccount) {

            await using var sqlCommand = new NpgsqlCommand(sql, dbConn);
            sqlCommand.Parameters.AddWithValue("@OwnerAccount", OwnerAccount);

            return await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateUserCoins(NpgsqlConnection dbConn, string OwnerAccount, int newCoinValue) {

            string updateCoinsSql = "UPDATE users SET coins = @NewCoins WHERE OwnerAccount = @OwnerAccount";
            await using var sqlCommand = new NpgsqlCommand(updateCoinsSql, dbConn);
            sqlCommand.Parameters.AddWithValue("@NewCoins", newCoinValue);
            sqlCommand.Parameters.AddWithValue("@OwnerAccount", OwnerAccount);
            int result = await sqlCommand.ExecuteNonQueryAsync();

            return result == 1;
        }

        public async Task<List<UserData>> RetrieveScoreboard() {

            var users = new List<UserData>();
            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();
            string query = "SELECT OwnerAccount, VictoryCount, LossCount, RatingPoints FROM users ORDER BY RatingPoints DESC";

            await using var sqlCommand = new NpgsqlCommand(query, databaseConnection);
            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            while (await dataReader.ReadAsync()) {

                users.Add(new UserData {

                    OwnerAccount = dataReader.GetString(0),
                    VictoryCount = dataReader.GetInt32(1),
                    LossCount = dataReader.GetInt32(2),
                    RatingPoints = dataReader.GetInt32(3)
                });
            }
            return users;
        }

        public async Task ResetDatabaseTables() {

            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            var tableNames = new[] { "users", "CardList", "packages" };

            foreach (var tableName in tableNames) {

                string sqlCommandText = $"TRUNCATE TABLE {tableName} CASCADE";
                await using var sqlCommand = new NpgsqlCommand(sqlCommandText, databaseConnection);
                await sqlCommand.ExecuteNonQueryAsync();
            }
        }


        public async Task<List<CardData>> FetchUserCards(UserData user) {

            return await FetchCardsWithQuery("SELECT PackageId  , CardName, ElementAttribute, AttackPower, CardType FROM CardList WHERE OwnerAccount=@OwnerAccount", user.OwnerAccount);
        }

        public async Task<List<CardData>> FetchCardsWithQuery(string query, string OwnerAccount) {

            var CardList = new List<CardData>();
            await using var databaseConnection = new NpgsqlConnection(connectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new NpgsqlCommand(query, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@OwnerAccount", OwnerAccount);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            while (await dataReader.ReadAsync()) {

                CardList.Add(new CardData {

                    PackageId = dataReader.GetString(0),
                    CardName = dataReader.GetString(1),
                    ElementAttribute = dataReader.GetString(2),
                    AttackPower = dataReader.GetDouble(3),
                    CardType = dataReader.GetString(4)
                });
            }
            return CardList;
        }

        public async Task<List<CardData>> RetrieveUserDeck(UserData user) {

            string query = @"
                SELECT c.PackageId  , c.CardName, c.ElementAttribute, c.AttackPower, c.CardType 
                FROM CardList c JOIN users u ON c.PackageId   = ANY(u.DeckList) 
                WHERE u.OwnerAccount=@OwnerAccount";

            return await FetchCardsWithQuery(query, user.OwnerAccount);
        }

        public async Task<bool> UpdateUserDeck(List<string> deck, string token) {

            string OwnerAccount = await FetchUsernameByToken(token);

            if (string.IsNullOrEmpty(OwnerAccount)) return false;

            return await UpdateDeckForUser(OwnerAccount, deck);
        }

        public async Task<string> FetchUsernameByToken(string token) {

            string query = "SELECT OwnerAccount FROM users WHERE token = @Token";
            await using var dbConn = new NpgsqlConnection(connectionString);
            await dbConn.OpenAsync();

            await using var sqlCommand = new NpgsqlCommand(query, dbConn);
            sqlCommand.Parameters.AddWithValue("@Token", token);

            return (string)await sqlCommand.ExecuteScalarAsync();
        }

        public async Task<bool> UpdateDeckForUser(string OwnerAccount, List<string> deck) {

            var deckString = "{" + string.Join(",", deck.Select(d => $"\"{d}\"")) + "}";
            string updateQuery = "UPDATE users SET DeckList = @Deck::text[] WHERE OwnerAccount = @OwnerAccount";

            await using var dbConn = new NpgsqlConnection(connectionString);
            await dbConn.OpenAsync();

            await using var updateCmd = new NpgsqlCommand(updateQuery, dbConn);
            updateCmd.Parameters.AddWithValue("@OwnerAccount", OwnerAccount);
            updateCmd.Parameters.AddWithValue("@Deck", deckString);

            int affectedRows = await updateCmd.ExecuteNonQueryAsync();
            return affectedRows == 1;
        }

        public async Task<bool> GenerateNewPackage(CardPackageData pack) {

            string insertQuery = "INSERT INTO packages (CardList) VALUES (@CardList)";

            await using var dbConn = new NpgsqlConnection(connectionString);
            await dbConn.OpenAsync();

            await using var sqlCommand = new NpgsqlCommand(insertQuery, dbConn);
            sqlCommand.Parameters.AddWithValue("@CardList", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, pack.CardList);

            int affectedRows = await sqlCommand.ExecuteNonQueryAsync();
            return affectedRows == 1;
        }


        public async Task<bool> InsertNewCards(List<CardData> CardList) {

            await using (var connection = new NpgsqlConnection(connectionString)) {

                await connection.OpenAsync();
                var insertQuery = new StringBuilder("INSERT INTO CardList (PackageId  , CardName, ElementAttribute, AttackPower, CardType) VALUES ");
                var valueStrings = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                for (int i = 0; i < CardList.Count; i++) {

                    var card = CardList[i];
                    var index = i.ToString();
                    valueStrings.Add($"(@PackageId  {index}, @CardName{index}, @ElementAttribute{index}, @AttackPower{index}, @CardType{index})");
                    parameters.Add(new NpgsqlParameter($"@PackageId  {index}", card.PackageId));
                    parameters.Add(new NpgsqlParameter($"@CardName{index}", card.CardName));
                    parameters.Add(new NpgsqlParameter($"@ElementAttribute{index}", card.ElementAttribute));
                    parameters.Add(new NpgsqlParameter($"@AttackPower{index}", card.AttackPower));
                    parameters.Add(new NpgsqlParameter($"@CardType{index}", card.CardType));
                }

                insertQuery.Append(string.Join(", ", valueStrings));
                await using (var cmd = new NpgsqlCommand(insertQuery.ToString(), connection)) {

                    cmd.Parameters.AddRange(parameters.ToArray());
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows == CardList.Count;
                }
            }
        }
    }
}
