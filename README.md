# Kağıt Uçak Simülatörü

Unity 6 (URP) ile yapılmış bir kağıt uçak fırlatma oyunu. Gücü ayarla, uçağı fırlat, sonsuz caddede yıldızları toplayarak olabildiğince uzağa süzül.

![Oyun görüntüsü](Docs/screenshot.png)

## Nasıl oynanır

| Kontrol | Ne yapar |
|---|---|
| **BAŞLA** | Giriş ekranından oyuna geçer |
| **SPACE** (basılı tut) | Güç çubuğunu doldurur; bırakınca uçak fırlar |
| **Fare** | Uçuş sırasında yönlendirme (sağ/sol dönüş, yukarı/aşağı burun) |
| **W / S** | Burnu yukarı / aşağı |
| **A / D** | Sola / sağa dönüş |
| **Esc** | İmleci serbest bırakır (tıklayınca tekrar kilitlenir) |

Uçak yere değdiğinde gidilen mesafe gösterilir; **TEKRAR OYNA** ile yeniden fırlatabilirsin.

## Özellikler

- **Güç çubuğu ve mesafe göstergesi**: güç ileri geri salınır, doğru anda bırakmak gerekir.
- **Hız yıldızları**: uçuş yolunda çıkar, içinden geçince hız verir.

  | Yıldız | Çıkma şansı | Hız kazancı |
  |---|---|---|
  | Mavi | %78 | +3 m/s |
  | Sarı | %18 | +6 m/s |
  | Kırmızı | %4 | +12 m/s |

- **Sonsuz cadde**: asfalt, şerit çizgileri, kaldırımlar ve ileride bina eklemek için boş arsalar (`LotLeft` / `LotRight` noktaları). Uçak ilerledikçe arkada kalan parçalar öne taşınır.
- **Kavisli dünya**: Subway Surfers'taki gibi yol ufka doğru aşağı bükülür, sonu siste kaybolur.
- **Koridor**: uçak caddenin kenarına yaklaşınca yumuşakça ortaya geri yönlendirilir.
- **Rüzgar izi**: uçarken kanat uçlarından ince, saydam izler çıkar.
- **Giriş ve sonuç ekranları**.

## Kurulum

1. Unity **6000.6.5f1** (veya uyumlu bir Unity 6 sürümü) ile projeyi aç.
2. `Assets/Scenes/SampleScene.unity` sahnesini aç.
3. **Play**'e bas.

Kullanılan paketler: Universal Render Pipeline, Input System.

## Proje yapısı

| Dosya | Görevi |
|---|---|
| `Assets/Scripts/PaperPlaneController.cs` | Fırlatma, uçuş fiziği, klavye/fare kontrolü, koridor sınırı |
| `Assets/Scripts/ThirdPersonCamera.cs` | Uçağı yumuşakça takip eden kamera |
| `Assets/Scripts/LaunchPowerHUD.cs` | Giriş ekranı, güç çubuğu, mesafe, sonuç ekranı, imleç kilidi |
| `Assets/Scripts/StarSpawner.cs` | Yıldızları uçağın önüne yerleştirir |
| `Assets/Scripts/StarPickup.cs` | Tek bir yıldız: döner, toplanınca hız verir |
| `Assets/Scripts/StreetBuilder.cs` | Sonsuz caddeyi oluşturur ve geri dönüştürür |
| `Assets/Scripts/CurvedWorld.cs` | Kavisli dünya efektinin gücünü ayarlar |
| `Assets/Scripts/WindTrail.cs` | Kanat ucu rüzgar izleri |
| `Assets/Shaders/CurvedWorld.shader` | Dünyayı kameradan uzaklaştıkça aşağı büken URP shader'ı |

## Ayarlanabilir değerler

Hepsi Inspector'dan değiştirilebilir:

- **Uçak hızı ve uçuş hissi**: `PaperPlane` → `PaperPlaneController` (fırlatma hızları, kaldırma/sürtünme, dönüş hızı, fare hassasiyeti).
- **Yıldızlar**: `Stars` → `StarSpawner` (aralık, renk başına şans ve hız kazancı, açılış yüksekliği).
- **Cadde ve kavis**: `Street` → `StreetBuilder` (genişlikler, parça sayısı) ve `CurvedWorld` (`curvature`).
- **Sis**: Window → Rendering → Lighting → Environment → Fog.
- **Rüzgar izi**: `PaperPlane` → `WindTrail` (saydamlık, kalınlık, uzunluk).
