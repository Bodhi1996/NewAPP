using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;
using NewAPP.Models;
using System.Reflection.PortableExecutable;
using System.Collections.ObjectModel;

namespace NewAPP.Services
{
    public class DatabaseService
    {
        private readonly string _connection;
        

        public DatabaseService ( string db = "users.db" ) // подключение к базе данных
        {
            _connection = $"Data Source = {db}";
            
            InitializeDatabase();
        }

        private void InitializeDatabase ( ) // создание базы данных, и ее таблиц.
        {
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();

                // Создание таблицы
                var createTable = connection.CreateCommand();
                createTable.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    Role TEXT DEFAULT 'User',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
                createTable.ExecuteNonQuery();

                // Проверка наличия пользователей
                var checkUsers = connection.CreateCommand();
                checkUsers.CommandText = "SELECT COUNT(*) FROM Users";
                long count = (long)checkUsers.ExecuteScalar();

                // Если нет пользователей - создаем тестовых
                if (count == 0)
                {
                    // Админ
                    var insertAdmin = connection.CreateCommand();
                    insertAdmin.CommandText = "INSERT INTO Users(Login, Password, Role) VALUES ('admin', @pass, 'Admin')";
                    insertAdmin.Parameters.AddWithValue("@pass", HashPassword("admin123"));
                    insertAdmin.ExecuteNonQuery();

                    // Обычный пользователь
                    var insertUser = connection.CreateCommand();
                    insertUser.CommandText = "INSERT INTO Users(Login, Password, Role) VALUES ('user', @pass, 'User')";
                    insertUser.Parameters.AddWithValue("@pass", HashPassword("user123"));
                    insertUser.ExecuteNonQuery();
                }
                //var dropTable = connection.CreateCommand();
                //dropTable.CommandText = "DROP TABLE IF EXISTS Nomenclature";
                //dropTable.ExecuteNonQuery();


                createTable.CommandText = @"CREATE TABLE IF NOT EXISTS nomenclature ( 
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Name TEXT UNIQUE NOT NULL,
                                            InternalArticle TEXT NOT NULL,
                                            ExternalArticle TEXT NOT NULL,
                                            Characteristic TEXT NOT NULL,
                                            SerialNumber TEXT NOT NULL,
                                            Unit TEXT NOT NULL,
                                            AddressCell TEXT NOT NULL,
                                            OldQuantity INTEGER DEFAULT 0,
                                            OperationTypeIn INTEGER DEFAULT 0,
                                            OperationTypeOut INTEGER DEFAULT 0,
                                             NewQuantity INTEGER DEFAULT 0,
                                             OperationDate DATETIME DEFAULT CURRENT_TIMESTAMP
            )";


                createTable.ExecuteNonQuery();

                var bolts = new List<(string Name, string InternalArticle, string ExternalArticle,
                                          string Characteristic, string SerialNumber, string Unit,
                                          string AddressCell, int OldQuantity, int OperationTypeIn,
                                          int OperationTypeOut, int NewQuantity)>

            {
                ("Болт М1", "111111", "131313", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М2", "222222", "121212", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М3", "333333", "111115", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М4", "444444", "101010", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М5", "555555", "999999", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М6", "666666", "888888", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М7", "777777", "777777", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М8", "888888", "666666", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М9", "999999", "555555", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М10", "101010", "444444", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М11", "111112", "333333", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М12", "131313", "222222", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90),
                ("Болт М13", "141414", "111111", "DIN7", "435435", "ШТ", "1-1-1", 100, 0, 10, 90)

            };

                // Вставка данных
                int added = 0;
                int skipped = 0;

                foreach (var bolt in bolts)
                {
                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = @"INSERT OR IGNORE INTO nomenclature 
                                            (Name, InternalArticle, ExternalArticle, Characteristic, 
                                             SerialNumber, Unit, AddressCell, OldQuantity, 
                                             OperationTypeIn, OperationTypeOut, NewQuantity, OperationDate) 
                                            VALUES ($name, $internalArticle, $externalArticle, 
                                                    $characteristic, $serialNumber, $unit, $addressCell, 
                                                    $oldQuantity, $operationTypeIn, $operationTypeOut, 
                                                    $newQuantity, $operationDate)";
                    insertCommand.Parameters.AddWithValue("$name", bolt.Name);
                    insertCommand.Parameters.AddWithValue("$internalArticle", bolt.InternalArticle);
                    insertCommand.Parameters.AddWithValue("$externalArticle", bolt.ExternalArticle);
                    insertCommand.Parameters.AddWithValue("$characteristic", bolt.Characteristic);
                    insertCommand.Parameters.AddWithValue("$serialNumber", bolt.SerialNumber);
                    insertCommand.Parameters.AddWithValue("$unit", bolt.Unit);
                    insertCommand.Parameters.AddWithValue("$addressCell", bolt.AddressCell);
                    insertCommand.Parameters.AddWithValue("$oldQuantity", bolt.OldQuantity);
                    insertCommand.Parameters.AddWithValue("$operationTypeIn", bolt.OperationTypeIn);
                    insertCommand.Parameters.AddWithValue("$operationTypeOut", bolt.OperationTypeOut);
                    insertCommand.Parameters.AddWithValue("$newQuantity", bolt.NewQuantity);
                    insertCommand.Parameters.AddWithValue("$operationDate", DateTime.Now);

                    int result = insertCommand.ExecuteNonQuery();
                    if (result > 0)
                        added++;
                    else
                        skipped++;
                }
                //nf,kbwf nthvbyfkjd
                createTable.CommandText = @"CREATE TABLE IF NOT EXISTS Terminals (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                IpAddress TEXT NOT NULL,
                Port INTEGER NOT NULL,
                UnitId INTEGER NOT NULL,
                IsConnected INTEGER DEFAULT 0,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
                createTable.ExecuteNonQuery();

                // Таблица датчиков
                createTable.CommandText = @"CREATE TABLE IF NOT EXISTS Sensors (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TerminalId INTEGER NOT NULL,
                SensorNumber INTEGER NOT NULL,
                Name TEXT NOT NULL,
                RegisterAddress INTEGER NOT NULL,
                Row INTEGER DEFAULT 0,
                Shelf INTEGER DEFAULT 0,
                Cell INTEGER DEFAULT 0,
                IsConnected INTEGER DEFAULT 0,
                Weight INTEGER DEFAULT 0,
                SelectedNomenclature TEXT,
                FOREIGN KEY (TerminalId) REFERENCES Terminals(Id) ON DELETE CASCADE,
                UNIQUE(TerminalId, SensorNumber)
                )";
                createTable.ExecuteNonQuery();
            }
        }

        private string HashPassword ( string password )
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public User Authenticate ( string login, string password )
        {
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT Id, Login, Password, Role, CreatedAt FROM Users 
                                           WHERE Login = @login";
                    command.Parameters.AddWithValue("@login", login);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.GetString(2);
                            string inputHash = HashPassword(password);

                            if (storedHash == inputHash)
                            {
                                return new User
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Login = reader.GetString(1),
                                    Role = reader.GetString(3),
                                    CreatedAt = reader.GetDateTime(4)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка аутентификации: {ex.Message}");
            }
            return null;
        }

        public bool AddUser ( string login, string password, string role )
        {
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO Users(Login, Password, Role)
                                           VALUES (@login, @pass, @role)";
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@pass", HashPassword(password));
                    command.Parameters.AddWithValue("@role", role);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                return false;
            }
        }

        public List<NomenclatureUnit> AllNum ( )
        {
            var result = new List<NomenclatureUnit>();
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();

                var incertResult = connection.CreateCommand();

                incertResult.CommandText = "SELECT Id, Name, InternalArticle, ExternalArticle, Characteristic, SerialNumber, Unit, AddressCell, OldQuantity, OperationTypeIn, OperationTypeOut, NewQuantity, OperationDate FROM nomenclature";

                using (var reader = incertResult.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(
                            new NomenclatureUnit
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                InternalArticle = reader.GetString(2),
                                ExternalArticle = reader.GetString(3),
                                Characteristic = reader.GetString(4),
                                SerialNamber = reader.GetString(5),
                                Unit = reader.GetString(6),
                                AddressCell = reader.GetString(7),
                                OldQuantity = reader.GetInt32(8).ToString(),
                                OperationTypeIn = reader.GetInt32(9).ToString(),
                                OperationTypeOut = reader.GetInt32(10).ToString(),
                                NewQuantity = reader.GetInt32(11).ToString(),
                                OperationDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12)

                            });
                    }
                }
            }
            return result;
        } //вывод всего содержания

        public bool AddNomenclature ( string name, string internalArticle, string externalArticle, string characteristic, string serialNumber, string unit, string addressCell, int oldQuantity, int operationTypeIn, int operationTypeOut, int newQuantity ) //добавление чего либо
        {
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();

                    var incertResult = connection.CreateCommand();
                    incertResult.CommandText = "INSERT INTO nomenclature (Name, InternalArticle, ExternalArticle, Characteristic, SerialNumber, Unit, AddressCell, OldQuantity, OperationTypeIn, OperationTypeOut, NewQuantity) VALUES (@name, @internalArticle, @externalArticle, @characteristic, @serialNumber, @unit, @addressCell, @oldQuantity, @operationTypeIn, @operationTypeOut, @newQuantity)";
                    incertResult.Parameters.AddWithValue(@"name", name);
                    incertResult.Parameters.AddWithValue(@"internalArticle", internalArticle);
                    incertResult.Parameters.AddWithValue(@"externalArticle", externalArticle);
                    incertResult.Parameters.AddWithValue(@"characteristic", characteristic);
                    incertResult.Parameters.AddWithValue(@"serialNumber", serialNumber);
                    incertResult.Parameters.AddWithValue(@"unit", unit);
                    incertResult.Parameters.AddWithValue(@"addressCell", addressCell);
                    incertResult.Parameters.AddWithValue("@oldQuantity", oldQuantity);
                    incertResult.Parameters.AddWithValue("@operationTypeIn",operationTypeIn);
                    incertResult.Parameters.AddWithValue("@operationTypeOut", operationTypeOut);
                    incertResult.Parameters.AddWithValue("@newQuantity", newQuantity);
                    incertResult.Parameters.AddWithValue("@operationDate", DateTime.Now);

                    incertResult.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public bool DeleteNomenclature ( string name )
        {
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();

                    var insertDelete = connection.CreateCommand();
                    insertDelete.CommandText = "DELETE FROM nomenclature WHERE Name = @name";
                    insertDelete.Parameters.AddWithValue("@name", name);

                    int rowAffected = insertDelete.ExecuteNonQuery();
                    return rowAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления номенклатуры: {ex.Message}");
                return false;
            }
        } //удаление

        public List<NomenclatureItems> SearchNomenclature ( string searchText )
        {
            var result = new List<NomenclatureItems>();
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();
                    var searchCommand = connection.CreateCommand();
                    searchCommand.CommandText = @"SELECT Id, Name, InternalArticle, ExternalArticle, 
                Characteristic, SerialNumber, Unit, AddressCell, OldQuantity, 
                OperationTypeIn, OperationTypeOut, NewQuantity
                FROM nomenclature
                WHERE Name LIKE @search
                ORDER BY Name";

                    searchCommand.Parameters.AddWithValue("@search", $"%{searchText}%");

                    using (var reader = searchCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new NomenclatureItems()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                InternalArticle = reader.GetString(2),
                                ExternalArticle = reader.GetString(3),
                                Characteristic = reader.GetString(4),
                                SerialNamber = reader.GetString(5),
                                Unit = reader.GetString(6),
                                AddressCell = reader.GetString(7),
                                // INTEGER → STRING для модели
                                OldQuantity = reader.GetInt32(8).ToString(),
                                OperationTypeIn = reader.GetInt32(9).ToString(),
                                OperationTypeOut = reader.GetInt32(10).ToString(),
                                NewQuantity = reader.GetInt32(11).ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка SearchNomenclature: {ex.Message}");
            }
            return result;
        } //поиск

        public bool DeleteUnit (string name, int amount )
        {
           using(var connection = new SqliteConnection(_connection))
            {
                connection.Open();
                var insertDel = connection.CreateCommand();
                insertDel.CommandText = @"SELECT NewQuantity
                                          FROM nomenclature
                                          WHERE Name LIKE @name";
                insertDel.Parameters.AddWithValue("@name", name );

                var startQuantity = 0;

                using(var reader = insertDel.ExecuteReader())
                {
                    if(reader.Read())
                    {
                        startQuantity = reader.GetInt32(0);
                    }
                    
                }
                if (startQuantity < 0) startQuantity = 0;

                var newQuantity = startQuantity - amount;
                var createTable = connection.CreateCommand();
                createTable.CommandText = @"UPDATE nomenclature
                                           SET NewQuantity = @newQuantity,
                                           OldQuantity = OldQuantity + @amount,
                                           OperationDate = @operationDate
                                           WHERE name = @name";

                createTable.Parameters.AddWithValue("@newQuantity", newQuantity);
                createTable.Parameters.AddWithValue("@amount", amount);
                createTable.Parameters.AddWithValue("@operationDate", DateTime.Now);
                createTable.Parameters.AddWithValue("@name", name);

                var result = createTable.ExecuteNonQuery();
                return result > 0;
 
            }
        }

        public List<Terminal> GetTerminals ( )
        {
            var terminals = new List<Terminal>();
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, IpAddress, Port, UnitId, IsConnected FROM Terminals ORDER BY Id";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var terminal = new Terminal
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IpAddress = reader.GetString(2),
                            Port = reader.GetInt32(3),
                            UnitId = (byte)reader.GetInt32(4),
                            IsConnected = reader.GetInt32(5) == 1,
                            Sensors = GetSensorsByTerminalId(reader.GetInt32(0))
                        };
                        terminals.Add(terminal);
                    }
                }
            }
            return terminals;
        }

        public ObservableCollection<Sensor> GetSensorsByTerminalId ( int terminalId )
        {
            var sensors = new ObservableCollection<Sensor>();
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT Id, SensorNumber, Name, RegisterAddress, Row, Shelf, Cell, 
                                      IsConnected, Weight, SelectedNomenclature 
                               FROM Sensors WHERE TerminalId = @terminalId ORDER BY SensorNumber";
                command.Parameters.AddWithValue("@terminalId", terminalId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sensor = new Sensor
                        {
                            Id = reader.GetInt32(0),
                            SensorNumber = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            RegisterAddress = (ushort)reader.GetInt32(3),
                            Row = reader.GetInt32(4),
                            Shelf = reader.GetInt32(5),
                            Cell = reader.GetInt32(6),
                            IsConnected = reader.GetInt32(7) == 1,
                            Weight = reader.GetInt32(8),
                            SelectedNomenclature = reader.IsDBNull(9) ? null : reader.GetString(9)
                        };
                        sensors.Add(sensor);
                    }
                }
            }
            return sensors;
        }

        public void SaveTerminal ( Terminal terminal )
        {
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();

                // Сохраняем терминал
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Terminals (Name, IpAddress, Port, UnitId, IsConnected) 
                               VALUES (@name, @ip, @port, @unitId, @isConnected);
                               SELECT last_insert_rowid()";
                command.Parameters.AddWithValue("@name", terminal.Name);
                command.Parameters.AddWithValue("@ip", terminal.IpAddress);
                command.Parameters.AddWithValue("@port", terminal.Port);
                command.Parameters.AddWithValue("@unitId", terminal.UnitId);
                command.Parameters.AddWithValue("@isConnected", terminal.IsConnected ? 1 : 0);

                int newId = Convert.ToInt32(command.ExecuteScalar());
                terminal.Id = newId;

                // Сохраняем датчики
                foreach (var sensor in terminal.Sensors)
                {
                    var sensorCommand = connection.CreateCommand();
                    sensorCommand.CommandText = @"INSERT INTO Sensors (TerminalId, SensorNumber, Name, RegisterAddress, 
                                          Row, Shelf, Cell, IsConnected, Weight, SelectedNomenclature) 
                                         VALUES (@terminalId, @sensorNumber, @name, @registerAddress,
                                                @row, @shelf, @cell, @isConnected, @weight, @selectedNomenclature)";
                    sensorCommand.Parameters.AddWithValue("@terminalId", terminal.Id);
                    sensorCommand.Parameters.AddWithValue("@sensorNumber", sensor.SensorNumber);
                    sensorCommand.Parameters.AddWithValue("@name", sensor.Name);
                    sensorCommand.Parameters.AddWithValue("@registerAddress", sensor.RegisterAddress);
                    sensorCommand.Parameters.AddWithValue("@row", sensor.Row);
                    sensorCommand.Parameters.AddWithValue("@shelf", sensor.Shelf);
                    sensorCommand.Parameters.AddWithValue("@cell", sensor.Cell);
                    sensorCommand.Parameters.AddWithValue("@isConnected", sensor.IsConnected ? 1 : 0);
                    sensorCommand.Parameters.AddWithValue("@weight", sensor.Weight);
                    sensorCommand.Parameters.AddWithValue("@selectedNomenclature", sensor.SelectedNomenclature ?? "");
                    sensorCommand.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTerminal ( Terminal terminal )
        {
            using (var connection = new SqliteConnection(_connection))
            {
                connection.Open();

                // Обновляем терминал
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Terminals SET Name = @name, IpAddress = @ip, 
                               Port = @port, UnitId = @unitId, IsConnected = @isConnected 
                               WHERE Id = @id";
                command.Parameters.AddWithValue("@name", terminal.Name);
                command.Parameters.AddWithValue("@ip", terminal.IpAddress);
                command.Parameters.AddWithValue("@port", terminal.Port);
                command.Parameters.AddWithValue("@unitId", terminal.UnitId);
                command.Parameters.AddWithValue("@isConnected", terminal.IsConnected ? 1 : 0);
                command.Parameters.AddWithValue("@id", terminal.Id);
                command.ExecuteNonQuery();

                // Удаляем старые датчики и добавляем новые
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Sensors WHERE TerminalId = @id";
                deleteCommand.Parameters.AddWithValue("@id", terminal.Id);
                deleteCommand.ExecuteNonQuery();

                foreach (var sensor in terminal.Sensors)
                {
                    var sensorCommand = connection.CreateCommand();
                    sensorCommand.CommandText = @"INSERT INTO Sensors (TerminalId, SensorNumber, Name, RegisterAddress, 
                                          Row, Shelf, Cell, IsConnected, Weight, SelectedNomenclature) 
                                         VALUES (@terminalId, @sensorNumber, @name, @registerAddress,
                                                @row, @shelf, @cell, @isConnected, @weight, @selectedNomenclature)";
                    sensorCommand.Parameters.AddWithValue("@terminalId", terminal.Id);
                    sensorCommand.Parameters.AddWithValue("@sensorNumber", sensor.SensorNumber);
                    sensorCommand.Parameters.AddWithValue("@name", sensor.Name);
                    sensorCommand.Parameters.AddWithValue("@registerAddress", sensor.RegisterAddress);
                    sensorCommand.Parameters.AddWithValue("@row", sensor.Row);
                    sensorCommand.Parameters.AddWithValue("@shelf", sensor.Shelf);
                    sensorCommand.Parameters.AddWithValue("@cell", sensor.Cell);
                    sensorCommand.Parameters.AddWithValue("@isConnected", sensor.IsConnected ? 1 : 0);
                    sensorCommand.Parameters.AddWithValue("@weight", sensor.Weight);
                    sensorCommand.Parameters.AddWithValue("@selectedNomenclature", sensor.SelectedNomenclature ?? "");
                    sensorCommand.ExecuteNonQuery();
                }
            }
        }

        public bool DeleteTerminal ( int terminalId )
        {
            try
            {
                using (var connection = new SqliteConnection(_connection))
                {
                    connection.Open();

                    // Сначала удаляем связанные датчики
                    var deleteSensors = connection.CreateCommand();
                    deleteSensors.CommandText = "DELETE FROM Sensors WHERE TerminalId = @id";
                    deleteSensors.Parameters.AddWithValue("@id", terminalId);
                    deleteSensors.ExecuteNonQuery();

                    // Затем удаляем терминал
                    var deleteTerminal = connection.CreateCommand();
                    deleteTerminal.CommandText = "DELETE FROM Terminals WHERE Id = @id";
                    deleteTerminal.Parameters.AddWithValue("@id", terminalId);

                    int rowsAffected = deleteTerminal.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления: {ex.Message}");
                return false;
            }
        }
        

    }
}