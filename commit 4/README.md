# 🏋️ FitLife Spor Merkezi Yönetim Sistemi

Modern bir ASP.NET Core MVC web uygulaması - Fitness merkezi yönetimi için tam kapsamlı çözüm.

## 📋 Özellikler

### 👥 Kullanıcı Yönetimi
- ✅ Üye kayıt ve giriş sistemi
- ✅ Rol tabanlı yetkilendirme (Admin / Member)
- ✅ Profil yönetimi

### 📅 Randevu Sistemi
- ✅ Online randevu oluşturma
- ✅ Antrenör müsaitlik kontrolü
- ✅ Çakışma engelleme
- ✅ Randevu onay/red/iptal

### 🤖 Yapay Zeka Özellikleri
- ✅ Kişiselleştirilmiş egzersiz programı önerisi
- ✅ Beslenme ve diyet planı oluşturma
- ✅ Görsel vücut analizi (fotoğraf yükleme)

### 👨‍💼 Admin Paneli
- ✅ Dashboard ile istatistikler
- ✅ Spor salonu yönetimi
- ✅ Hizmet yönetimi
- ✅ Antrenör yönetimi
- ✅ Üye yönetimi
- ✅ Randevu yönetimi

### 🔌 REST API
- ✅ Randevu API'leri
- ✅ Antrenör API'leri

---

## 🚀 Projeyi Çalıştırma

### Gereksinimler

1. **.NET 8.0 SDK** - [İndir](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** (LocalDB yeterli)

### Kurulum Adımları

#### 1. Proje dizinine gidin:
```powershell
cd "c:\Users\admin\OneDrive - ogr.sakarya.edu.tr\Masaüstü\FitnessCenterManagement"
```

#### 2. Paketleri yükleyin:
```powershell
dotnet restore
```

#### 3. Veritabanını oluşturun:
```powershell
# Veritabanı otomatik oluşturulacak (EnsureCreated kullanılıyor)
# Alternatif olarak migration kullanabilirsiniz:
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 4. Projeyi çalıştırın:
```powershell
dotnet run
```

#### 5. Tarayıcıda açın:
```
https://localhost:5001
veya
http://localhost:5000
```

---

## 🔐 Varsayılan Kullanıcılar

| Rol | E-posta | Şifre |
|-----|---------|-------|
| **Admin** | b231210383@sakarya.edu.tr | sau |

> 📝 Not: Yeni üyeler kayıt olduktan sonra otomatik olarak "Member" rolü alır.

---

## 📁 Proje Yapısı

```
FitnessCenterManagement/
├── Controllers/           # MVC Controller'lar
│   ├── Api/              # REST API Controller'lar
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── AIController.cs
│   ├── AppointmentController.cs
│   └── HomeController.cs
├── Data/                  # Veritabanı
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs  # Seed data
├── Models/
│   └── Entities/         # Entity modelleri
├── Services/             # AI servisleri
├── ViewModels/           # View model'ler
├── Views/                # Razor view'lar
│   ├── Account/
│   ├── Admin/
│   ├── AI/
│   ├── Appointment/
│   ├── Home/
│   └── Shared/
└── wwwroot/              # Statik dosyalar
```

---

## 🛠️ Teknolojiler

- **Backend:** ASP.NET Core 8.0 MVC
- **Veritabanı:** SQL Server + Entity Framework Core
- **Kimlik Doğrulama:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5, Bootstrap Icons
- **AI:** OpenAI API (opsiyonel, mock data ile de çalışır)

---

## ⚙️ Yapılandırma

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FitnessCenterDb;Trusted_Connection=True;"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
  }
}
```

> 💡 OpenAI API anahtarı olmadan da proje çalışır (mock data kullanır).

---

## 📱 Ekran Görüntüleri

### Ana Sayfa
- Modern hero section
- Hizmetler listesi
- AI özellik tanıtımı
- Antrenör kadrosu

### Admin Panel
- İstatistik dashboard
- CRUD işlemleri
- Randevu yönetimi

### AI Özellikleri
- Egzersiz programı önerisi
- Beslenme planı
- Görsel analiz

---

## 👨‍💻 Geliştirici

**Ömer Yazıcıgil**  
Sakarya Üniversitesi  
Öğrenci No: b231210383

---

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

