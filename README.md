# Rehberlik Sistemi

## Genel Bakış
Rehberlik Sistemi, ASP.NET Core 9.0 ile geliştirilmiş web tabanlı bir rehberlik ve danışmanlık yönetim sistemidir. Yöneticiler, öğretmenler ve öğrenciler arasındaki iletişimi kolaylaştıran, öğrenci rehberlik faaliyetlerinin yönetilmesini sağlayan kapsamlı bir platform sunar.

## Temel Özellikler
* **Rol Tabanlı Erişim Kontrolü:** ASP.NET Core Identity kullanılarak güvenli kimlik doğrulama ve yetkilendirme sağlanır. Önceden tanımlanmış roller:
  * **Yönetici (Admin):** Tam sistem erişimi, kullanıcı yönetimi ve yapılandırma işlemleri.
  * **Öğretmen (Rehber):** Öğrenci kayıtlarına erişim, rehberlik oturumları ve notların yönetimi.
  * **Öğrenci:** Kişisel rehberlik geçmişine erişim ve rehber öğretmenlerle iletişim.
* **Veritabanı Yönetimi:** Güçlü veri depolama ve sorgulama için Entity Framework Core 9.0 ve SQL Server kullanılmaktadır.
* **Modern Web Arayüzü:** ASP.NET Core MVC ile oluşturulmuş, duyarlı (responsive) ve kullanıcı dostu bir deneyim sunar.

## Kullanılan Teknolojiler
* **Backend:** ASP.NET Core 9.0 (MVC)
* **ORM:** Entity Framework Core 9.0
* **Veritabanı:** Microsoft SQL Server
* **Kimlik Doğrulama / Yetkilendirme:** ASP.NET Core Identity
* **Frontend:** HTML, CSS, JavaScript (Razor Views)

## Proje Yapısı
* `Controllers/`: Kullanıcı isteklerini işleyen MVC denetleyicilerini içerir (örn: `AdminController`, `TeacherController`, `StudentController`, `AccountController`).
* `Core/`: Temel iş mantığı, varlık (entity) sınıfları ve arayüzleri (interface) barındırır.
* `Data/`: Entity Framework `ApplicationDbContext` ve veritabanı tohumlama (seeding) mantığını içerir.
* `Models/`: Uygulama genelinde kullanılan veri transfer nesneleri (DTO) ve görünüm modelleri (view model) yer alır.
* `Views/`: Kullanıcı arayüzünü oluşturan Razor görünüm dosyalarını içerir.
* `wwwroot/`: CSS, JavaScript ve görseller gibi statik dosyaları barındırır.

## Başlarken

### Ön Gereksinimler
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* SQL Server (LocalDB veya ayrı bir sunucu)

### Kurulum ve Yapılandırma
1. **Depoyu klonlayın:**
   ```bash
   git clone <depo-adresi>
   cd <depo-dizini>
   ```

2. **Veritabanı Bağlantısını Yapılandırın:**
   `RehberlikSistemi.Web/appsettings.Development.json` (veya `appsettings.json`) dosyasını açarak `DefaultConnection` bağlantı dizesini kendi SQL Server sunucunuza göre güncelleyin.

3. **Veritabanı Migrasyonlarını Uygulayın:**
   Proje dizinine gidin ve veritabanı şemasını oluşturmak için Entity Framework Core migrasyonlarını çalıştırın:
   ```bash
   cd RehberlikSistemi.Web
   dotnet ef database update
   ```
   *Not: EF Core CLI araçları yüklü değilse, `dotnet tool install --global dotnet-ef` komutuyla global olarak yükleyebilirsiniz.*

4. **Uygulamayı Çalıştırın:**
   .NET CLI kullanarak uygulamayı başlatın:
   ```bash
   dotnet run
   ```
   Uygulama başlatıldıktan sonra, konsol çıktısında belirtilen adres üzerinden erişebilirsiniz (genellikle `https://localhost:5001` veya `http://localhost:5000`).

### Varsayılan Kullanıcılar (Tohumlanmış)
Uygulama ilk çalıştırmada varsayılan kullanıcıları ve rolleri otomatik olarak oluşturabilir. Başlangıçta tanımlanan kullanıcı bilgileri için `Program.cs` dosyasındaki `DbSeeder.SeedRolesAndUsersAsync` metodunu inceleyebilirsiniz.

## Lisans
[Lisans bilgisi buraya eklenecektir]
