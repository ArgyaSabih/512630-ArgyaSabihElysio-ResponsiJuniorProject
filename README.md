# Developer Team Performance Tracker (DTPT)

Aplikasi Windows Forms untuk mencatat dan mengelola data developer serta menghitung performa tim berdasarkan fitur yang diselesaikan dan bug yang ditemukan.

## Deskripsi

Aplikasi ini memungkinkan pengguna untuk:
- **Mencatat** data developer beserta proyek yang dikerjakan
- **Mengedit** informasi developer yang sudah ada
- **Menghapus** data developer dari sistem
- **Menghitung otomatis** skor performa dan gaji berdasarkan status kontrak

## 📊 Struktur Database

### Tabel `proyek`
| Kolom | Tipe | Keterangan |
|-------|------|------------|
| id_proyek | SERIAL | Primary Key |
| nama_proyek | VARCHAR(100) | Nama proyek |
| budget | DECIMAL(15,2) | Budget proyek |

### Tabel `developer`
| Kolom | Tipe | Keterangan |
|-------|------|------------|
| id_dev | SERIAL | Primary Key |
| id_proyek | INTEGER | Foreign Key ke proyek |
| nama_dev | VARCHAR(100) | Nama developer |
| status_kontrak | VARCHAR(20) | 'Full Time' atau 'Freelance' |
| fitur_selesai | INTEGER | Jumlah fitur yang dikerjakan |
| jumlah_bug | INTEGER | Jumlah bug yang ditemukan |

##  Konsep OOP yang Diterapkan

### 1. Abstraction
```csharp
public abstract class Developer : BaseEntity
{
    public abstract decimal HitungSkor();
    public abstract decimal HitungGaji();
}
```

### 2. Inheritance
```csharp
public class DeveloperFullTime : Developer { ... }
public class DeveloperFreelance : Developer { ... }
```

### 3. Polymorphism
```csharp
// Factory method untuk membuat instance yang tepat
public static Developer Create(string statusKontrak)
{
    if (statusKontrak == "Full Time")
        return new DeveloperFullTime();
    else
        return new DeveloperFreelance();
}
```

### 4. Encapsulation
```csharp
public abstract class BaseEntity
{
    protected int _id;
    public int Id { get => _id; set => _id = value; }
}
```

## Cara Menjalankan

### Prasyarat
1. Visual Studio 2022
2. PostgreSQL 15 atau lebih baru
3. pgAdmin 4

### Langkah-langkah

1. **Clone/Download Project**

2. **Setup Database PostgreSQL**
   - Buka pgAdmin4
   - Buat database baru dengan nama `sabih_responsi`
   - Jalankan script `database_setup.sql` untuk membuat tabel dan function

3. **Konfigurasi Connection String**
   
   Edit file `DatabaseHelper.cs` jika diperlukan:
   ```csharp
   private string connectionString = "Host=localhost;Port=5432;Database=sabih_responsi;Username=postgres;Password=your_password";
   ```

4. **Build dan Run**
   ```powershell
   # Build project
   & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ResponsiJuniorProject.sln /p:Configuration=Debug
   
   # Jalankan aplikasi
   .\bin\Debug\ResponsiJuniorProject.exe
   ```

## Struktur Project

```
ResponsiJuniorProject/
├── Models/
│   ├── BaseEntity.cs      # Abstract base class
│   ├── Developer.cs       # Abstract Developer + subclasses
│   └── Proyek.cs          # Model proyek
├── bin/
│   └── Debug/             # Output executable
├── App.config             # Konfigurasi aplikasi
├── DatabaseHelper.cs      # Helper class untuk database
├── database_setup.sql     # Script setup database
├── Form1.cs               # Main form logic
├── Form1.Designer.cs      # Form designer
├── Program.cs             # Entry point
└── README.md              # Dokumentasi
```

## Catatan Penting

- Pastikan PostgreSQL service sudah berjalan sebelum menjalankan aplikasi
- Database harus sudah dibuat dan script `database_setup.sql` sudah dijalankan
- Jika terjadi error koneksi, periksa kembali connection string di `DatabaseHelper.cs`
