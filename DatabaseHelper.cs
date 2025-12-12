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
            catch (Exception ex)
            {
                // Show actual error for debugging
                System.Windows.Forms.MessageBox.Show("Database Error: " + ex.Message + "\n\nInner: " + (ex.InnerException?.Message ?? "none"), 
                    "Debug Info", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
        /// Menggunakan fungsi PostgreSQL hitung_skor dan hitung_gaji
        /// </summary>
        public DataTable GetDeveloperDataTable()
        {
            var dt = new DataTable();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                // Query menggunakan fungsi PostgreSQL untuk menghitung skor dan gaji
                string query = @"
                    SELECT 
                        d.id_dev AS ""ID"",
                        d.nama_dev AS ""Nama"",
                        p.nama_proyek AS ""Proyek"",
                        d.status_kontrak AS ""Status"",
                        d.fitur_selesai AS ""Fitur"",
                        d.jumlah_bug AS ""Bug"",
                        hitung_skor(d.status_kontrak, d.fitur_selesai, d.jumlah_bug) AS ""SKOR"",
                        hitung_gaji(d.status_kontrak, d.fitur_selesai, d.jumlah_bug) AS ""TOTAL GAJI""
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
        /// Hitung total gaji semua developer dalam satu proyek menggunakan fungsi PostgreSQL
        /// </summary>
        public decimal GetTotalGajiByProyek(int idProyek)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT COALESCE(SUM(hitung_gaji(status_kontrak, fitur_selesai, jumlah_bug)), 0)
                    FROM developer
                    WHERE id_proyek = @id_proyek";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_proyek", idProyek);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        /// <summary>
        /// Hitung gaji developer menggunakan fungsi PostgreSQL
        /// </summary>
        public decimal HitungGajiDeveloper(string statusKontrak, int fiturSelesai, int jumlahBug)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT hitung_gaji(@status, @fitur, @bug)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@status", statusKontrak);
                    cmd.Parameters.AddWithValue("@fitur", fiturSelesai);
                    cmd.Parameters.AddWithValue("@bug", jumlahBug);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        /// <summary>
        /// Cek apakah operasi akan menyebabkan proyek underbudget
        /// </summary>
        public bool CekBudgetProyek(int idProyek, decimal gajiBaruDeveloper, int? excludeIdDev = null)
        {
            var proyek = GetProyekById(idProyek);
            if (proyek == null) return false;

            decimal totalGajiSaatIni = GetTotalGajiByProyek(idProyek);

            // Jika update, kurangi gaji developer yang sedang di-update
            if (excludeIdDev.HasValue)
            {
                var devLama = GetDeveloperById(excludeIdDev.Value);
                if (devLama != null)
                {
                    decimal gajiLama = HitungGajiDeveloper(devLama.StatusKontrak, devLama.FiturSelesai, devLama.JumlahBug);
                    totalGajiSaatIni -= gajiLama;
                }
            }

            decimal totalGajiBaru = totalGajiSaatIni + gajiBaruDeveloper;
            return totalGajiBaru <= proyek.Budget;
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
                            // Gunakan factory method untuk membuat instance (polymorphism)
                            var developer = Developer.Create(
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetInt32(4),
                                reader.GetInt32(5)
                            );
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
                            // Gunakan factory method untuk membuat instance (polymorphism)
                            return Developer.Create(
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetInt32(4),
                                reader.GetInt32(5)
                            );
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
