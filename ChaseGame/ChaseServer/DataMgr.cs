using System;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ChaseServer
{
    class DataMgr
    {
        MySqlConnection sqlConn;
        BinaryFormatter formatter;

        public static DataMgr instance;
        public DataMgr()
        {
            instance = this;
            formatter = new BinaryFormatter();
            Connect();
        }

        public void Connect()
        {
            string connStr = "Database=game;DataSource=127.0.0.1;";
            connStr += "User Id=root;Password=abcd3077118;port=3306;";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Console.Write("[DataMgr]Connect " + e.Message);
                return;
            }
        }

        public bool IsSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\']");
        }

        private int CanRegister(string id)
        {
            if (!IsSafeStr(id))
                return 401;         // 用户名不合法
            string cmdStr = string.Format("select * from user where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                if (hasRows)
                    return 400;     // 用户名已存在
                return 200;         // 用户名合法
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
                return 500;         // 服务器查询数据库出错
            }
        }

        public int Register(string id, string pw)
        {
            if (!IsSafeStr(pw))
                return 402;         // 密码不合法
            int idstate = CanRegister(id);
            if (idstate != 200)
                return idstate;

            string cmdStr = string.Format("insert into user set  id='{0}',pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                cmd.ExecuteNonQuery();
                return 200;         // 成功
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]Register" + e.Message);
                return 501;         // 服务器写入数据库出错
            }
        }

        public bool CreatePlayer(string id)
        {
            if (!IsSafeStr(id))
                return false;
            
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();

            string cmdStr = string.Format("insert into player set id='{0}', data=@data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
                return false;
            }
        }

        public int CheckPassWord(string id, string pw)
        {
            if (!IsSafeStr(id))
                return 400;
            if (!IsSafeStr(pw))
                return 401;

            string cmdStr = string.Format("select * from user where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                int rtstate = 200;
                if (!dataReader.HasRows)
                    rtstate = 403;
                else
                {
                    dataReader.Read();
                    if (dataReader.GetString(dataReader.GetOrdinal("pw")) != pw)
                        rtstate = 404;
                }
                dataReader.Close();
                return rtstate;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CheckPassWord " + e.Message);
                return 500;
            }
        }

        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = null;
            if (!IsSafeStr(id))
                return playerData;
            string cmdStr = string.Format("select * from player where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer;
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerData;
                }
                dataReader.Read();

                long len = dataReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                dataReader.GetBytes(1, 0, buffer, 0, (int)len);
                dataReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
                return playerData;
            }
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化 " + e.Message);
                return playerData;
            }
        }

        public int SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData playerData = player.data;

            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
                return 502;
            }
            byte[] byteArr = stream.ToArray();

            string formatStr = "update player set data=@data where id='{0}';";
            string cmdStr = string.Format(formatStr, id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return 200;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 写入 " + e.Message);
                return 501;
            }
        }
    }
}
