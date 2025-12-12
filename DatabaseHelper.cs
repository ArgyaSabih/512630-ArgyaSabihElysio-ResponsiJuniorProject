using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using ResponsiJuniorProject.Models;

namespace ResponsiJuniorProject
{
    /// <summary>
    /// Database Helper class for PostgreSQL connection and CRUD operations
    /// </summary>
    public class DatabaseHelper
    {
        // Connection string - modify according to your PostgreSQL setup
        private readonly string connectionString;

        public DatabaseHelper()
        {
            // Default connection string - adjust host, port, username, and password as needed
            connectionString = "Host=localhost;Port=5432;Database=sabih_responsi;Username=postgres;Password=informatika";
        }

        public DatabaseHelper(string host, string database, string username, string password, int port = 5432)
        {
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #region Proyek Operations

        /// <summary>
        /// Get all projects
        /// </summary>
        public List<Proyek> GetAllProyek()
        {
            var proyekList = new List<Proyek>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_proyek, nama_proyek, client, budget FROM proyek ORDER BY id_proyek", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var proyek = new Proyek
                            {
                                IdProyek = reader.GetInt32(0),
                                NamaProyek = reader.GetString(1),
                                Client = reader.GetString(2),
                                Budget = reader.GetDecimal(3)
                            };
                            proyekList.Add(proyek);
                        }
                    }
                }
            }

            return proyekList;
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        public Proyek GetProyekById(int idProyek)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_proyek, nama_proyek, client, budget FROM proyek WHERE id_proyek = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idProyek);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Proyek
                            {
                                IdProyek = reader.GetInt32(0),
                                NamaProyek = reader.GetString(1),
                                Client = reader.GetString(2),
                                Budget = reader.GetDecimal(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region Developer Operations

        /// <summary>
        /// Get all developers with project information for DataGridView
        /// </summary>
        public DataTable GetDeveloperDataTable()
        {
            var dt = new DataTable();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        d.id_dev AS ""ID"",
                        d.nama_dev AS ""Nama Developer"",
                        d.status_kontrak AS ""Status Kontrak"",
                        p.nama_proyek AS ""Nama Proyek"",
                        d.fitur_selesai AS ""Fitur Selesai"",
                        d.jumlah_bug AS ""Jumlah Bug""
                    FROM developer d
                    INNER JOIN proyek p ON d.id_proyek = p.id_proyek
                    ORDER BY d.id_dev";

                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// Get all developers
        /// </summary>
        public List<Developer> GetAllDevelopers()
        {
            var developerList = new List<Developer>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_dev, id_proyek, nama_dev, status_kontrak, fitur_selesai, jumlah_bug FROM developer ORDER BY id_dev", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var developer = new Developer
                            {
                                IdDev = reader.GetInt32(0),
                                IdProyek = reader.GetInt32(1),
                                NamaDev = reader.GetString(2),
                                StatusKontrak = reader.GetString(3),
                                FiturSelesai = reader.GetInt32(4),
                                JumlahBug = reader.GetInt32(5)
                            };
                            developerList.Add(developer);
                        }
                    }
                }
            }

            return developerList;
        }

        /// <summary>
        /// Get developer by ID
        /// </summary>
        public Developer GetDeveloperById(int idDev)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_dev, id_proyek, nama_dev, status_kontrak, fitur_selesai, jumlah_bug FROM developer WHERE id_dev = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idDev);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Developer
                            {
                                IdDev = reader.GetInt32(0),
                                IdProyek = reader.GetInt32(1),
                                NamaDev = reader.GetString(2),
                                StatusKontrak = reader.GetString(3),
                                FiturSelesai = reader.GetInt32(4),
                                JumlahBug = reader.GetInt32(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Insert new developer
        /// </summary>
        public bool InsertDeveloper(Developer developer)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO developer (id_proyek, nama_dev, status_kontrak, fitur_selesai, jumlah_bug) 
                                    VALUES (@id_proyek, @nama_dev, @status_kontrak, @fitur_selesai, @jumlah_bug)";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_proyek", developer.IdProyek);
                        cmd.Parameters.AddWithValue("@nama_dev", developer.NamaDev);
                        cmd.Parameters.AddWithValue("@status_kontrak", developer.StatusKontrak);
                        cmd.Parameters.AddWithValue("@fitur_selesai", developer.FiturSelesai);
                        cmd.Parameters.AddWithValue("@jumlah_bug", developer.JumlahBug);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Update existing developer
        /// </summary>
        public bool UpdateDeveloper(Developer developer)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE developer 
                                    SET id_proyek = @id_proyek, 
                                        nama_dev = @nama_dev, 
                                        status_kontrak = @status_kontrak, 
                                        fitur_selesai = @fitur_selesai, 
                                        jumlah_bug = @jumlah_bug 
                                    WHERE id_dev = @id_dev";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_dev", developer.IdDev);
                        cmd.Parameters.AddWithValue("@id_proyek", developer.IdProyek);
                        cmd.Parameters.AddWithValue("@nama_dev", developer.NamaDev);
                        cmd.Parameters.AddWithValue("@status_kontrak", developer.StatusKontrak);
                        cmd.Parameters.AddWithValue("@fitur_selesai", developer.FiturSelesai);
                        cmd.Parameters.AddWithValue("@jumlah_bug", developer.JumlahBug);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Delete developer by ID
        /// </summary>
        public bool DeleteDeveloper(int idDev)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM developer WHERE id_dev = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idDev);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
