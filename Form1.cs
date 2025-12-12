using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ResponsiJuniorProject.Models;

namespace ResponsiJuniorProject
{
    public partial class Form1 : Form
    {
        private DatabaseHelper dbHelper;
        private List<Proyek> proyekList;
        private int selectedDeveloperId = -1;

        public Form1()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Test database connection
            if (!dbHelper.TestConnection())
            {
                MessageBox.Show("Gagal terhubung ke database PostgreSQL!\n\nPastikan:\n1. PostgreSQL sudah berjalan\n2. Database 'developer_tracker' sudah dibuat\n3. Username dan password sudah benar\n\nSilakan jalankan script database_setup.sql di pgAdmin4 terlebih dahulu.", 
                    "Koneksi Database Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadProyekComboBox();
            LoadStatusKontrakComboBox();
            LoadFiturSelesaiComboBox();
            LoadJumlahBugComboBox();
            LoadDeveloperData();
        }

        /// <summary>
        /// Load project data to ComboBox
        /// </summary>
        private void LoadProyekComboBox()
        {
            try
            {
                proyekList = dbHelper.GetAllProyek();
                comboBox1.Items.Clear();
                comboBox1.DisplayMember = "NamaProyek";
                comboBox1.ValueMember = "IdProyek";

                foreach (var proyek in proyekList)
                {
                    comboBox1.Items.Add(proyek);
                }

                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading proyek: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load status kontrak options to ComboBox
        /// </summary>
        private void LoadStatusKontrakComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Freelance");
            comboBox2.Items.Add("Full Time");
            comboBox2.SelectedIndex = 0;
        }

        /// <summary>
        /// Load fitur selesai options to ComboBox (0-20)
        /// </summary>
        private void LoadFiturSelesaiComboBox()
        {
            comboBox3.Items.Clear();
            for (int i = 0; i <= 20; i++)
            {
                comboBox3.Items.Add(i.ToString());
            }
            comboBox3.SelectedIndex = 0;
        }

        /// <summary>
        /// Load jumlah bug options to ComboBox (0-20)
        /// </summary>
        private void LoadJumlahBugComboBox()
        {
            comboBox4.Items.Clear();
            for (int i = 0; i <= 20; i++)
            {
                comboBox4.Items.Add(i.ToString());
            }
            comboBox4.SelectedIndex = 0;
        }

        /// <summary>
        /// Load developer data to DataGridView
        /// </summary>
        private void LoadDeveloperData()
        {
            try
            {
                DataTable dt = dbHelper.GetDeveloperDataTable();
                dataGridView1.DataSource = dt;

                // Auto-resize columns
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                
                // Hide ID column if exists
                if (dataGridView1.Columns.Contains("ID"))
                {
                    dataGridView1.Columns["ID"].Visible = false;
                }

                // Format kolom TOTAL GAJI sebagai currency
                if (dataGridView1.Columns.Contains("TOTAL GAJI"))
                {
                    dataGridView1.Columns["TOTAL GAJI"].DefaultCellStyle.Format = "N0";
                    dataGridView1.Columns["TOTAL GAJI"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Format kolom SKOR
                if (dataGridView1.Columns.Contains("SKOR"))
                {
                    dataGridView1.Columns["SKOR"].DefaultCellStyle.Format = "N2";
                    dataGridView1.Columns["SKOR"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clear all input fields
        /// </summary>
        private void ClearFields()
        {
            textBox1.Text = "";
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 0) comboBox2.SelectedIndex = 0;
            if (comboBox3.Items.Count > 0) comboBox3.SelectedIndex = 0;
            if (comboBox4.Items.Count > 0) comboBox4.SelectedIndex = 0;
            selectedDeveloperId = -1;
        }

        /// <summary>
        /// Validate input fields
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Nama Developer harus diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return false;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Pilih Proyek terlebih dahulu!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Pilih Status Kontrak terlebih dahulu!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox2.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// INSERT button click - Add new developer
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                var proyek = (Proyek)comboBox1.SelectedItem;
                string statusKontrak = comboBox2.SelectedItem.ToString();
                int fiturSelesai = int.Parse(comboBox3.SelectedItem.ToString());
                int jumlahBug = int.Parse(comboBox4.SelectedItem.ToString());

                // Hitung gaji yang akan ditambahkan menggunakan fungsi PostgreSQL
                decimal gajiBaru = dbHelper.HitungGajiDeveloper(statusKontrak, fiturSelesai, jumlahBug);

                // Cek apakah budget proyek mencukupi
                if (!dbHelper.CekBudgetProyek(proyek.IdProyek, gajiBaru))
                {
                    MessageBox.Show($"Tidak dapat menambah developer!\n\nProyek '{proyek.NamaProyek}' akan menjadi UNDERBUDGET.\nBudget proyek: Rp {proyek.Budget:N0}\nGaji developer baru: Rp {gajiBaru:N0}", 
                        "Budget Tidak Mencukupi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Gunakan factory method untuk membuat developer (polymorphism)
                var developer = Developer.Create(statusKontrak);
                developer.IdProyek = proyek.IdProyek;
                developer.NamaDev = textBox1.Text.Trim();
                developer.FiturSelesai = fiturSelesai;
                developer.JumlahBug = jumlahBug;

                if (dbHelper.InsertDeveloper(developer))
                {
                    MessageBox.Show("Data developer berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDeveloperData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Gagal menambahkan data developer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// UPDATE button click - Update selected developer
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedDeveloperId == -1)
            {
                MessageBox.Show("Pilih data developer yang akan diupdate dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            try
            {
                var proyek = (Proyek)comboBox1.SelectedItem;
                string statusKontrak = comboBox2.SelectedItem.ToString();
                int fiturSelesai = int.Parse(comboBox3.SelectedItem.ToString());
                int jumlahBug = int.Parse(comboBox4.SelectedItem.ToString());

                // Hitung gaji baru menggunakan fungsi PostgreSQL
                decimal gajiBaru = dbHelper.HitungGajiDeveloper(statusKontrak, fiturSelesai, jumlahBug);

                // Cek apakah budget proyek mencukupi (exclude developer yang sedang di-update)
                if (!dbHelper.CekBudgetProyek(proyek.IdProyek, gajiBaru, selectedDeveloperId))
                {
                    MessageBox.Show($"Tidak dapat mengupdate developer!\n\nProyek '{proyek.NamaProyek}' akan menjadi UNDERBUDGET.\nBudget proyek: Rp {proyek.Budget:N0}\nGaji developer baru: Rp {gajiBaru:N0}", 
                        "Budget Tidak Mencukupi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Gunakan factory method untuk membuat developer (polymorphism)
                var developer = Developer.Create(statusKontrak);
                developer.IdDev = selectedDeveloperId;
                developer.IdProyek = proyek.IdProyek;
                developer.NamaDev = textBox1.Text.Trim();
                developer.FiturSelesai = fiturSelesai;
                developer.JumlahBug = jumlahBug;

                if (dbHelper.UpdateDeveloper(developer))
                {
                    MessageBox.Show("Data developer berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDeveloperData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Gagal mengupdate data developer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// DELETE button click - Delete selected developer
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedDeveloperId == -1)
            {
                MessageBox.Show("Pilih data developer yang akan dihapus dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Apakah Anda yakin ingin menghapus data developer ini?", "Konfirmasi Hapus", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (dbHelper.DeleteDeveloper(selectedDeveloperId))
                    {
                        MessageBox.Show("Data developer berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDeveloperData();
                        ClearFields();
                    }
                    else
                    {
                        MessageBox.Show("Gagal menghapus data developer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// DataGridView cell click - Select developer for edit/delete
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Get the ID from the hidden column
                selectedDeveloperId = Convert.ToInt32(row.Cells["ID"].Value);

                // Fill the form fields
                textBox1.Text = row.Cells["Nama"].Value.ToString();

                // Set Status Kontrak
                string statusKontrak = row.Cells["Status"].Value.ToString();
                comboBox2.SelectedItem = statusKontrak;

                // Set Proyek
                string namaProyek = row.Cells["Proyek"].Value.ToString();
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    var proyek = (Proyek)comboBox1.Items[i];
                    if (proyek.NamaProyek == namaProyek)
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                // Set Fitur Selesai
                string fiturSelesai = row.Cells["Fitur"].Value.ToString();
                comboBox3.SelectedItem = fiturSelesai;

                // Set Jumlah Bug
                string jumlahBug = row.Cells["Bug"].Value.ToString();
                comboBox4.SelectedItem = jumlahBug;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void label7_Click(object sender, EventArgs e)
        {
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
