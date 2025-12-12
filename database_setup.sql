-- ============================================================
-- Database Setup Script for Developer Team Performance Tracker
-- Jalankan script ini di pgAdmin4
-- ============================================================

-- ============================================================
-- LANGKAH 1: Buat database baru di pgAdmin4
-- Klik kanan pada "Databases" -> Create -> Database
-- Nama database: sabih_responsi
-- Kemudian klik "Save"
-- ============================================================

-- ============================================================
-- LANGKAH 2: Jalankan query berikut di Query Tool
-- Klik kanan pada database "sabih_responsi" -> Query Tool
-- Copy semua query di bawah ini dan jalankan
-- ============================================================

-- Hapus tabel jika sudah ada (untuk reset data)
DROP TABLE IF EXISTS developer CASCADE;
DROP TABLE IF EXISTS proyek CASCADE;
DROP VIEW IF EXISTS v_developer_performance;

-- ============================================================
-- ERD: Tabel PROYEK (Entitas 1)
-- Relasi: One-to-Many dengan Developer
-- ============================================================
CREATE TABLE proyek (
    id_proyek SERIAL PRIMARY KEY,
    nama_proyek VARCHAR(100) NOT NULL,
    client VARCHAR(100) NOT NULL,
    budget DECIMAL(15, 2) NOT NULL
);

-- ============================================================
-- ERD: Tabel DEVELOPER (Entitas 2)
-- Relasi: Many-to-One dengan Proyek (id_proyek sebagai FK)
-- ============================================================
CREATE TABLE developer (
    id_dev SERIAL PRIMARY KEY,
    id_proyek INTEGER NOT NULL,
    nama_dev VARCHAR(100) NOT NULL,
    status_kontrak VARCHAR(20) NOT NULL CHECK (status_kontrak IN ('Freelance', 'Full Time')),
    fitur_selesai INTEGER DEFAULT 0 CHECK (fitur_selesai >= 0),
    jumlah_bug INTEGER DEFAULT 0 CHECK (jumlah_bug >= 0),
    CONSTRAINT fk_proyek FOREIGN KEY (id_proyek) 
        REFERENCES proyek(id_proyek) ON DELETE CASCADE ON UPDATE CASCADE
);

-- ============================================================
-- Insert Data Proyek (Fixed - sesuai wireframe)
-- 5 Proyek dengan data yang sudah ditentukan
-- ============================================================
INSERT INTO proyek (nama_proyek, client, budget) VALUES
('Web Company Profile', 'CV Sejahtera', 8000000.00),
('Sistem Parkir QR', 'Dinas Perhubungan', 15000000.00),
('IoT Agriculture App', 'Tani Maju Indonesia', 25000000.00),
('E-Commerce Startup', 'PT Maju Mundur', 100000000.00),
('AI Fraud Detection', 'Unicorn Fintech', 150000000.00);

-- ============================================================
-- Insert Data Developer (Minimal 5 developer)
-- Data awal saat aplikasi dibuka
-- ============================================================
INSERT INTO developer (id_proyek, nama_dev, status_kontrak, fitur_selesai, jumlah_bug) VALUES
(1, 'Budi Speed', 'Full Time', 5, 2),
(2, 'WongLiyoRetiOpo', 'Full Time', 8, 3),
(3, 'Visca City', 'Freelance', 4, 0),
(4, 'Kipli Kopling', 'Full Time', 10, 5),
(5, 'Asep Freezer', 'Freelance', 6, 2);

-- ============================================================
-- View untuk menampilkan data performa developer
-- Sesuai dengan kebutuhan DataGridView pada aplikasi
-- ============================================================
CREATE OR REPLACE VIEW v_developer_performance AS
SELECT 
    d.id_dev,
    d.nama_dev AS "Nama Developer",
    d.status_kontrak AS "Status Kontrak",
    p.nama_proyek AS "Nama Proyek",
    d.fitur_selesai AS "Fitur Selesai",
    d.jumlah_bug AS "Jumlah Bug"
FROM developer d
INNER JOIN proyek p ON d.id_proyek = p.id_proyek
ORDER BY d.id_dev;

-- ============================================================
-- Query untuk verifikasi data
-- ============================================================
SELECT * FROM proyek;
SELECT * FROM developer;
SELECT * FROM v_developer_performance;

-- ============================================================
-- CATATAN KONEKSI:
-- Host: localhost
-- Port: 5432
-- Database: sabih_responsi
-- Username: postgres
-- Password: postgres
-- ============================================================
