# Kağıt Uçak Simülatörü

Unity 6 (URP) ile yapılmış bir kağıt uçak fırlatma oyunu. Kuleden gücü ayarlayıp uçağı fırlat, dünyanın 6 ünlü şehrinden geçen virajlı caddede trafiğe ve binalara çarpmadan, yıldızları toplayarak olabildiğince uzağa süzül.

![Oyun görüntüsü](Docs/screenshot.png)

## Nasıl oynanır

| Kontrol | Ne yapar |
|---|---|
| **BAŞLA** | Giriş ekranından oyuna geçer |
| **MAĞAZA** | Giriş ekranından mağazayı açar (altınla kozmetik al / kullan) |
| **SPACE** (basılı tut) | Güç çubuğunu doldurur; bırakınca uçak fırlar |
| **Fare** | Uçuş sırasında yönlendirme (sağ/sol dönüş, yukarı/aşağı burun) |
| **W / S** | Burnu yukarı / aşağı |
| **A / D** | Sola / sağa dönüş |
| **Esc** | Duraklatma menüsü: DEVAM ET, ANA MENÜ, ses ayarı |

Uçak yere indiğinde gidilen mesafe (cadde boyunca) gösterilir. Bir araca ya da binaya çarparsa oyun **"ÇARPTIN!"** ile biter. Toplanan yıldızların altını sonuç ekranında hesaba eklenir. **TEKRAR OYNA** ile yeniden fırlatabilirsin.

## Özellikler

- **Fırlatma kulesi**: caddenin ortasında, merdivenli ve korkuluklu 3 m'lik bir kule; güç çubuğu ileri geri salınır, doğru anda bırakmak gerekir.
- **6 şehir**: New York → Paris → Londra → Roma → Tokyo → Dubai, sonra baştan. Her şehir 320 m sürer, şehirler arasında 40 m'lik parklar var. Her şehrin kendi binaları, simge yapısı, sokak eşyaları, zemin ve sis renkleri var.
- **Virajlı, sonsuz cadde**: 16 m'lik 4 şeritli cadde rastgele yerlerde 30–60° yumuşakça sağa/sola kıvrılır; binalar, kaldırımlar ve trafik virajla birlikte döner. Uçak ilerledikçe arkada kalan parçalar öne taşınır.
- **Gidiş-geliş trafiği**: sol şeritlerden araçlar karşıdan gelir, sağ şeritlerde aynı yöne yavaş giderler. Sedan, hatchback, spor araba, panelvan, kamyon, otobüs; New York'ta sarı taksiler, Londra'da siyah taksi ve iki katlı kırmızı otobüsler.
- **Çarpışma**: araçlara ve binalara çarpmak oyunu bitirir; zemine inmek normal bitiştir.
- **Stall (hız kaybı)**: burnu sürekli yukarıda tutunca uçak yavaşlar; 7 m/s'nin altında kanatlar taşımayı bırakır, burun düşer ve uçak dalışa geçer. Burnu kaldırmayı bırakıp 15 m/s'ye kadar hızlanırsa toparlanır; stall'da yere çarpmak **"ÇARPTIN!"** sayılır.
- **Geri dönüş yok**: uçağın burnu caddenin yönünden en fazla 70° sapabilir; arkada kalan harita geri dönüştürüldüğü için geri uçulamaz.
- **Hız yıldızları**: uçuş yolunda çıkar, parlar ve içinden geçince hız ve altın verir. Altın atış bitince (iniş ya da çarpma) eklenir; ANA MENÜ ile yarıda bırakılan atış altın vermez.

  | Yıldız | Çıkma şansı | Hız kazancı | Altın |
  |---|---|---|---|
  | Mavi | %78 | +6 m/s | 1 |
  | Sarı | %18 | +12 m/s | 3 |
  | Kırmızı | %4 | +24 m/s | 6 |

- **Mağaza**: giriş ekranındaki MAĞAZA'da altınla sadece görünüşü değiştiren kozmetikler alınır (uçuş fiziği aynı kalır). Bir atış ortalama 10-20 altın getirir. Altın, alınanlar ve kullanılanlar kaydedilir.

  | Kategori | Eşyalar (altın) |
  |---|---|
  | Renk | Klasik (0), Gök Mavisi (150), Nane Yeşili (150), Gül Pembe (200), Gece Siyahı (250), Altın Sarısı (300) |
  | Şekil | Klasik (0), Ok (400), Kırlangıç (600), Planör (800), Avcı (1000), Vatoz (1200) |
  | Efekt | Klasik iz (0), Gökkuşağı İzi (450), Altın İz (600) |

  Şekiller `Assets/Resources/Planes/<id>.json` dosyalarından kurulur (kalınlıklı kâğıt panelleri); dosyası olmayan şekil mağazada görünmez. Claude Design için hazır prompt'lar: `Docs/claude-design-plane-prompts.md`.
- **Arka plan şehir**: arsaların arkasında aynı şehrin binalarından ikinci, biraz daha yüksek bir sıra ve daha geride sise karışan büyük silüetler var; parklarda ve virajların iç tarafında boş kalır.

- **Kavisli dünya**: Subway Surfers'taki gibi dünya ufka doğru aşağı bükülür; caddenin sonu sise, sis de gökyüzüne kesintisiz karışır.
- **Gökyüzü**: AllSky Free'den "Epic_BlueSunset" (bulutlu mavi gökyüzü).
- **Koridor**: uçak caddenin kenarına yaklaşınca, virajlarda da, yumuşakça ortaya geri yönlendirilir.
- **Rüzgar izi**: uçarken kanat uçlarından ince, saydam izler çıkar.
- **Ses ayarı**: Esc menüsündeki SES çubuğu ana ses seviyesini ayarlar ve hatırlar; ileride eklenecek tüm sesler buna uyar.

## Kurulum

1. Unity **6000.6.5f1** (veya uyumlu bir Unity 6 sürümü) ile projeyi aç.
2. `Assets/Scenes/SampleScene.unity` sahnesini aç.
3. **Play**'e bas.

Kullanılan paketler: Universal Render Pipeline, Input System. Resimler Git LFS ile tutulur (`git lfs install` sonra klonla).

**AllSky Free:** Repoda sadece kullanılan gökyüzü (`Assets/AllSkyFree/Epic_BlueSunset`) var; paketin geri kalanı `.gitignore`'da. Diğer gökyüzlerini denemek istersen Package Manager → My Assets → "AllSky Free - 10 Sky / Skybox Set" → Import.

## Proje yapısı

| Dosya | Görevi |
|---|---|
| `Assets/Scripts/PaperPlaneController.cs` | Fırlatma, uçuş fiziği, klavye/fare kontrolü, koridor sınırı, iniş ve çarpma |
| `Assets/Scripts/ThirdPersonCamera.cs` | Uçağı yumuşakça takip eden kamera |
| `Assets/Scripts/LaunchPowerHUD.cs` | Giriş ekranı, güç çubuğu, mesafe, sonuç ekranı (kazanılan altın), Esc menüsü, imleç kilidi |
| `Assets/Scripts/ShopMenu.cs` | Mağaza ekranı: renk / şekil / efekt sekmeleri, satın al / kullan |
| `Assets/Scripts/CosmeticCatalog.cs` | Mağazadaki kozmetiklerin listesi ve fiyatları |
| `Assets/Scripts/PlayerWallet.cs` | Altın, alınan ve kullanılan kozmetikler (kaydedilir) |
| `Assets/Scripts/PlaneCosmetics.cs` | Kullanılan rengi, şekli ve iz efektini uçağa uygular; şekil JSON'larından 3D model kurar |
| `Assets/Scripts/PlaneShapeData.cs` | Uçak şekli JSON formatının sınıfları |
| `Assets/Resources/Planes/*.json` | Mağaza uçak şekilleri (paneller, malzeme, kanat ucu) |
| `Assets/Scripts/GameSettings.cs` | Ana ses seviyesi (kaydedilir, `AudioListener`'a uygulanır) |
| `Assets/Scripts/StreetPath.cs` | Caddenin yolu: düz bölümler ve rastgele virajlar, yol üzerindeki konum/ilerleme hesapları |
| `Assets/Scripts/StreetBuilder.cs` | Caddeyi yol boyunca kıvrık parçalar halinde kurar ve geri dönüştürür |
| `Assets/Scripts/CityBuilder.cs` | Arsalara şehir binalarını ve sokak eşyalarını yerleştirir, arkalarına ikinci sıra ve silüet, şehir renkleri ve sis |
| `Assets/Scripts/CityMeshBuilder.cs` | JSON'daki basit şekilleri (kutu, silindir, koni, çatı, kubbe, pencere, cephe) tek mesh'e çevirir |
| `Assets/Scripts/CityData.cs` | Şehir, bina, eşya ve araç verilerinin sınıfları |
| `Assets/Scripts/JsonModel.cs` | JSON'dan tek bir model kurar (fırlatma kulesi) |
| `Assets/Scripts/TrafficSpawner.cs` | Gidiş-geliş trafiğini üretir, yürütür ve geri dönüştürür |
| `Assets/Scripts/Vehicle.cs` | Tek bir araç; uçak çarparsa oyunu bitirir |
| `Assets/Scripts/Obstacle.cs` | Çarpınca oyunu bitiren engelleri (binalar) işaretler |
| `Assets/Scripts/StarSpawner.cs` | Parlayan yıldızları yol boyunca uçağın önüne yerleştirir |
| `Assets/Scripts/StarPickup.cs` | Tek bir yıldız: döner, parlar, toplanınca hız ve altın verir |
| `Assets/Scripts/CurvedWorld.cs` | Kavisli dünya efektinin gücünü ayarlar |
| `Assets/Scripts/WindTrail.cs` | Kanat ucu rüzgar izleri |
| `Assets/Shaders/CurvedWorld.shader` | Dünyayı kameradan uzaklaştıkça aşağı büken URP shader'ı |
| `Assets/Shaders/CurvedGlow.shader` | Yıldızların etrafındaki hale ve ışınlar |
| `Assets/Shaders/SkyboxHorizonFog.shader` | Gökyüzünü ufukta sis rengine karıştıran skybox |
| `Assets/Data/Cities/*.json` | Şehirlerin binaları, eşyaları ve renkleri; `park.json` şehirler arası parklar |
| `Assets/Data/vehicles.json` | Araç tipleri, renkleri ve hangi şehirde çıktıkları |
| `Assets/Data/launch_pad.json` | Fırlatma kulesinin modeli |

## Ayarlanabilir değerler

Hepsi Inspector'dan ya da JSON dosyalarından değiştirilebilir:

- **Uçak hızı ve uçuş hissi**: `PaperPlane` → `PaperPlaneController` (fırlatma hızları, kaldırma/sürtünme, dönüş hızı, fare hassasiyeti, koridor genişliği, en fazla sapma açısı, stall hızları).
- **Cadde ve virajlar**: `Street` → `StreetBuilder` (genişlikler, şerit aralığı, viraj sıklığı ve açısı) ve `CurvedWorld` (`curvature`).
- **Şehirler**: `Street` → `CityBuilder` (şehir sırası, bölge uzunluğu, sisin gökyüzüne ne kadar yaklaşacağı, arka sıra ve silüet mesafe/ölçekleri); binaların kendisi `Assets/Data/Cities/*.json`.
- **Trafik**: `Traffic` → `TrafficSpawner` (şeritler, hızlar, araç aralığı, en fazla araç); araç tipleri `Assets/Data/vehicles.json`.
- **Yıldızlar**: `Stars` → `StarSpawner` (aralık, renk başına şans, hız kazancı ve altın, parlama).
- **Mağaza**: fiyatlar, renkler ve iz efektleri `Assets/Scripts/CosmeticCatalog.cs`; yeni kozmetik = listeye bir satır.
- **Gökyüzü ve sis**: `Assets/Materials/SkyAllSky.mat` (gökyüzü resmi, ufuk geçişi); sis Window → Rendering → Lighting → Environment → Fog.
- **Rüzgar izi**: `PaperPlane` → `WindTrail` (saydamlık, kalınlık, uzunluk).

## Kaynaklar

- Gökyüzü: [AllSky Free - 10 Sky / Skybox Set](https://assetstore.unity.com/) (Unity Asset Store, ücretsiz).
