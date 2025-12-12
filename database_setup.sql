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
DROP FUNCTION IF EXISTS hitung_skor(VARCHAR, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS hitung_gaji(VARCHAR, INTEGER, INTEGER);

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
-- FUNGSI 1: hitung_skor
-- Menghitung skor performa developer berdasarkan status kontrak
-- Full Time: Skor = 10 × Fitur - 5 × Bug
-- Freelance: Skor = 100 × (1 - (2 × Bug) / (3 × Fitur))
-- Skor minimum adalah 0
-- ============================================================
CREATE OR REPLACE FUNCTION hitung_skor(
    p_status_kontrak VARCHAR,
    p_fitur_selesai INTEGER,
    p_jumlah_bug INTEGER
) RETURNS DECIMAL AS $$
DECLARE
    v_skor DECIMAL;
BEGIN
    IF p_status_kontrak = 'Full Time' THEN
        -- Rumus Full Time: Skor = 10 × Fitur - 5 × Bug
        v_skor := (10.0 * p_fitur_selesai) - (5.0 * p_jumlah_bug);
    ELSE
        -- Rumus Freelance: Skor = 100 × (1 - (2 × Bug) / (3 × Fitur))
        IF p_fitur_selesai = 0 THEN
            v_skor := 0;
        ELSE
            v_skor := 100.0 * (1.0 - (2.0 * p_jumlah_bug) / (3.0 * p_fitur_selesai));
        END IF;
    END IF;
    
    -- Skor tidak boleh negatif
    IF v_skor < 0 THEN
        v_skor := 0;
    END IF;
    
    RETURN ROUND(v_skor, 2);
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- FUNGSI 2: hitung_gaji
-- Menghitung total gaji developer berdasarkan status kontrak dan skor
-- Full Time: Gaji = Gaji Pokok (Rp5.000.000) + Skor × Rp20.000
-- Freelance: 
--   Skor >= 80: Rp500.000 × Fitur
--   50 <= Skor < 80: Rp400.000 × Fitur
--   Skor < 50: Rp250.000 × Fitur
-- ============================================================
CREATE OR REPLACE FUNCTION hitung_gaji(
    p_status_kontrak VARCHAR,
    p_fitur_selesai INTEGER,
    p_jumlah_bug INTEGER
) RETURNS DECIMAL AS $$
DECLARE
    v_skor DECIMAL;
    v_gaji DECIMAL;
    v_gaji_pokok DECIMAL := 5000000.00;
BEGIN
    -- Hitung skor terlebih dahulu
    v_skor := hitung_skor(p_status_kontrak, p_fitur_selesai, p_jumlah_bug);
    
    IF p_status_kontrak = 'Full Time' THEN
        -- Rumus Full Time: Gaji = Gaji Pokok + Skor × Rp20.000
        v_gaji := v_gaji_pokok + (v_skor * 20000.00);
    ELSE
        -- Rumus Freelance berdasarkan skor
        IF v_skor >= 80 THEN
            v_gaji := 500000.00 * p_fitur_selesai;
        ELSIF v_skor >= 50 THEN
            v_gaji := 400000.00 * p_fitur_selesai;
        ELSE
            v_gaji := 250000.00 * p_fitur_selesai;
        END IF;
    END IF;
    
    RETURN v_gaji;
END;
$$ LANGUAGE plpgsql;

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
(1, 'Budi Speed', 'Full Time', 10, 2),
(2, 'WongLiyoRetiOpo', 'Full Time', 8, 3),
(3, 'Visca City', 'Freelance', 5, 0),
(4, 'Kipli Kopling', 'Full Time', 10, 5),
(4, 'Asep Freezer', 'Freelance', 6, 2);

-- ============================================================
-- View untuk menampilkan data performa developer dengan Skor dan Gaji
-- Menggunakan fungsi PostgreSQL hitung_skor dan hitung_gaji
-- ============================================================
CREATE OR REPLACE VIEW v_developer_performance AS
SELECT 
    d.id_dev,
    d.id_proyek,
    d.nama_dev AS "Nama Developer",
    p.nama_proyek AS "Nama Proyek",
    d.status_kontrak AS "Status Kontrak",
    d.fitur_selesai AS "Fitur Selesai",
    d.jumlah_bug AS "Jumlah Bug",
    hitung_skor(d.status_kontrak, d.fitur_selesai, d.jumlah_bug) AS "Skor",
    hitung_gaji(d.status_kontrak, d.fitur_selesai, d.jumlah_bug) AS "Total Gaji"
FROM developer d
INNER JOIN proyek p ON d.id_proyek = p.id_proyek
ORDER BY d.id_dev;

-- ============================================================
-- Query untuk verifikasi data
-- ============================================================
SELECT * FROM proyek;
SELECT * FROM developer;
SELECT * FROM v_developer_performance;

-- Test fungsi PostgreSQL
SELECT 
    hitung_skor('Full Time', 10, 2) AS skor_fulltime,
    hitung_gaji('Full Time', 10, 2) AS gaji_fulltime,
    hitung_skor('Freelance', 5, 0) AS skor_freelance,
    hitung_gaji('Freelance', 5, 0) AS gaji_freelance;

-- ============================================================
-- CATATAN KONEKSI:
-- Host: localhost
-- Port: 5432
-- Database: sabih_responsi
-- Username: postgres
-- Password: postgres
-- ============================================================
