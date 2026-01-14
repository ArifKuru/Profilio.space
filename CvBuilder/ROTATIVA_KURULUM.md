# Rotativa PDF Kurulum Talimatları

## Önemli Not
Rotativa paketi PDF oluşturmak için **wkhtmltopdf** adlı bir araç kullanır. Bu aracın sisteminizde yüklü olması gerekir.

## Kurulum Adımları

### 1. NuGet Paketini Yükleme

Visual Studio'da Package Manager Console'u açın ve şu komutu çalıştırın:

```powershell
Install-Package Rotativa
```

Veya Visual Studio'da Solution Explorer'da projeye sağ tıklayıp "Manage NuGet Packages" seçeneğini kullanarak "Rotativa" paketini arayıp yükleyin.

### 2. wkhtmltopdf Kurulumu

Rotativa'nın çalışması için **wkhtmltopdf** aracının yüklü olması gerekir.

#### Windows için:
1. [wkhtmltopdf indirme sayfasına](https://wkhtmltopdf.org/downloads.html) gidin
2. Windows için uygun sürümü indirin (32-bit veya 64-bit)
3. İndirilen .exe dosyasını çalıştırıp kurulumu tamamlayın
4. Kurulum genellikle `C:\Program Files\wkhtmltopdf\` dizinine yapılır

#### Alternatif Yöntem (Rotativa.Core kullanarak):
Eğer wkhtmltopdf kurulumu sorun yaratıyorsa, Rotativa.Core paketini kullanabilirsiniz:

```powershell
Install-Package Rotativa.Core
```

Bu paket, wkhtmltopdf'i otomatik olarak projenize dahil eder.

### 3. Proje Yapılandırması

Rotativa paketi yüklendikten sonra, `bin` klasörüne `Rotativa` klasörü eklenecektir. Bu klasör içinde wkhtmltopdf dosyaları bulunur.

### 4. Test Etme

Kurulum tamamlandıktan sonra:
1. Projeyi derleyin (Build)
2. Uygulamayı çalıştırın
3. `/admin/buildcv` sayfasına gidin
4. "PDF Olarak İndir" butonuna tıklayın
5. PDF dosyasının indirildiğini kontrol edin

## Sorun Giderme

### Hata: "wkhtmltopdf not found"
- wkhtmltopdf'in doğru kurulduğundan emin olun
- PATH ortam değişkenine wkhtmltopdf'in kurulum dizinini ekleyin
- Veya Rotativa.Core paketini kullanın

### PDF'te görseller görünmüyor
- ViewCv.cshtml'deki görsel yollarının doğru olduğundan emin olun
- Mutlak URL kullanıldığından emin olun (Request.Url.GetLeftPart kullanılıyor)

### PDF boş geliyor
- ViewCv action'ının doğru çalıştığını kontrol edin
- Tarayıcıda `/admin/buildcv/viewcv` adresini açarak HTML'in doğru görüntülendiğini kontrol edin

## Notlar

- Production ortamında, wkhtmltopdf'in sunucuda da kurulu olması gerekir
- IIS'te çalıştırırken, uygulama havuzunun doğru izinlere sahip olduğundan emin olun
- PDF oluşturma işlemi CPU yoğun bir işlemdir, büyük dosyalar için timeout ayarlarını kontrol edin

