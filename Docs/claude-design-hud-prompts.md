# Claude Design: HUD (kâğıt / el işi teması)

Her bölüm tek başına bir prompt'tur. **Hepsini aynı Claude Design projesinde, sırayla** gönder: 1. prompt ortak kiti (fontlar, paneller, butonlar, ikonlar) tanımlar, sonrakiler aynı asset id'lerini kullanır. Ortak stil ve teknik kısım her prompt'ta tekrarlanıyor, böylece her biri tek başına da anlaşılır.

Her prompt'un çıktısı: mockup + en sonda tek bir JSON (düzen, renkler, fontlar, SVG asset'ler). JSON'ları ve proje linkini getir, oyuna ben aktaracağım (`LaunchPowerHUD.cs`, `ShopMenu.cs`).

| # | Ekran | JSON `screen` |
|---|---|---|
| 1 | Ortak HUD kiti | `hud_kit` |
| 2 | Giriş ekranı | `start_screen` |
| 3 | Uçuş HUD'u (güç çubuğu, mesafe) | `flight_hud` |
| 4 | Sonuç ekranı | `result_screen` |
| 5 | Duraklatma menüsü | `pause_menu` |
| 6 | Mağaza | `shop_menu` |

---

## 1) Ortak HUD kiti (`hud_kit`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz bir dizi prompt'la ekran ekran tasarlanacak; **bu ilk prompt, sonraki bütün ekranların kullanacağı ortak kit.**

**Bu prompt'ta istediğim — ortak kit:**
- **Fontlar:** başlık ve gövde fontunu seç, hangi boyutlarda kullanılacağını öner (başlık / alt başlık / buton / gövde / küçük not).
- **Paneller:** büyük kâğıt kart (menü kutusu, sonuç kartı), küçük etiket kartı (sayaçlar için), kraft şerit (başlık bandı); hepsi 9-slice.
- **Butonlar:** birincil (altın), ikincil (krem), tehlike/uyarı (kırmızı); her biri için normal, hover, pressed, disabled hâlleri.
- **Sekme:** seçili ve seçili olmayan hâl.
- **İlerleme çubuğu:** arka plan (track) + dolum (fill) sprite'ları; dolum rengi koddan değişecek (yeşil → sarı → kırmızı), bu yüzden dolum sprite'ı beyaz/gri tonlu olsun, renk `color` ile verilsin.
- **Kaydırıcı:** track, fill, handle.
- **Süsler:** 2–3 farklı bant parçası, ataç, katlanmış köşe, kâğıt dokusu (tekrarlanabilir küçük desen).
- **İkonlar (64×64):** altın para, yıldız, kilit, ses (hoparlör), geri oku.
- **Mockup:** bütün kiti tek sayfada, etiketli bir "sticker sheet" olarak göster; butonların her hâli yan yana.

JSON'da `screen`: `"hud_kit"`. Bu prompt'ta `elements` boş kalabilir (ya da kitteki örnekler `"decor": true` ile); asıl çıktı `fonts`, `colors`, genişletilmiş stil token'ları ve **`assets`**. Asset id'lerini şöyle adlandır ki sonraki ekranlar kullanabilsin: `panel_large`, `panel_label`, `strip_kraft`, `button_primary`, `button_secondary`, `button_danger` (+ hâl sprite'ları gerekiyorsa `_hover`, `_pressed`, `_disabled` ekleriyle), `tab_on`, `tab_off`, `bar_track`, `bar_fill`, `slider_track`, `slider_fill`, `slider_handle`, `tape_1`, `tape_2`, `clip`, `corner_fold`, `paper_texture`, `icon_coin`, `icon_star`, `icon_lock`, `icon_sound`, `icon_back`.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.

---

## 2) Giriş ekranı (`start_screen`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz ekran ekran tasarlanıyor; önceki prompt'ta ortak bir kit (fontlar, paneller, butonlar, ikonlar) tanımlandıysa onu kullan.

**Bu ekran — Giriş ekranı:** Oyun açılınca ilk görülen ekran; tam ekranı kaplar, arkadaki 3D sahne görünmez. Oyuncu buradan oyuna başlar ya da mağazaya gider.

Öğeler (id → metin):
- `StartScreen` — kök, tam ekran arka plan. Şu an indigo → magenta → turuncu dikey gün batımı gradyanı; bu his korunsun, üstüne kâğıt kolaj öğeleri (kesilmiş bulutlar, şehir silüeti şeridi, uçuş yolu kesik çizgisi, küçük kâğıt uçak) eklenebilir. Gradyanı SVG asset olarak ver.
- `Title` — "KAĞIT UÇAK" (oyunun logosu gibi; büyük, el yazısı/marker, kâğıda yazılmış ya da kâğıttan kesilmiş harfler).
- `Controls` — "SPACE basılı tut: güç   •   Fare / WASD: yönlendir   •   Yıldızlar hız verir" (küçük not kâğıdı gibi).
- `StartButton` — "BAŞLA" (birincil, en dikkat çekici).
- `ShopButton` — "MAĞAZA" (ikincil).
- `Gold` — sağ üstte "ALTIN: 6105" (altın para ikonuyla; sayı 6 haneye kadar çıkabilir).

İstenen hâller: normal ekran; `StartButton` hover hâli; ekran açılırken öğelerin giriş animasyonu (not olarak).

JSON'da `screen`: `"start_screen"`.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.

---

## 3) Uçuş HUD'u (`flight_hud`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz ekran ekran tasarlanıyor; önceki prompt'ta ortak bir kit (fontlar, paneller, butonlar, ikonlar) tanımlandıysa onu kullan.

**Bu ekran — Oyun içi HUD:** Oyun sırasında 3D sahnenin (şehir caddesi, binalar, gökyüzü, uçağın arkasından kamera) **önünde** duran küçük parçalar. Sahneyi kapatmamalı; açık gökyüzünde de koyu binaların önünde de okunmalı. Mockup'ta arka planı şehir caddesi + gün batımı göğü gibi temsil et (sadece referans, JSON'a girmez).

Öğeler (id → metin):
- `PowerBar` — alt ortada, alttan ~80 px yukarıda; şu an 500×28 civarı (değiştirebilirsin). Sadece uçak fırlatma kulesinde beklerken görünür, fırlatınca solarak kaybolur.
  - `Fill` — dolum; genişliği 0–%100 arası koddan değişir, rengi de koddan: %0 yeşil `#4DD94D` → %50 sarı `#FFD933` → %100 kırmızı `#F23326`. Dolum ileri-geri salınır (güç doğru anda bırakılmalı).
  - `Label` — çubuğun hemen üstünde, iki hâl: güç dolmuyorken "Fırlatmak için SPACE basılı tut", dolarken "GÜÇ %72".
- `DistanceMeter` — üst ortada, üstten ~40 px aşağıda; şu an 360×70. Uçuş boyunca görünür.
  - `Text` — "128 m" (5 haneye kadar: "12840 m").

İstenen hâller: (a) kulede bekleme — `Label` ipucu metniyle, dolum boş; (b) güç dolarken — "GÜÇ %72", dolum %72 sarı-turuncu; (c) uçuş sırasında — sadece `DistanceMeter`. Ayrıca güç çubuğunun "doğru anda bırak" hissini artıracak, uGUI ile yapılabilir küçük bir fikir (ör. %90–100 bölgesinde işaret) öner.

JSON'da `screen`: `"flight_hud"`.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.

---

## 4) Sonuç ekranı (`result_screen`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz ekran ekran tasarlanıyor; önceki prompt'ta ortak bir kit (fontlar, paneller, butonlar, ikonlar) tanımlandıysa onu kullan.

**Bu ekran — Sonuç ekranı:** Uçak yere indikten ya da bir araca/binaya çarptıktan ~0.8 saniye sonra solarak açılır. Arkada durmuş oyun sahnesi var; şu an %90 siyahla karartılıyor (bu oranı sen belirle).

Öğeler (id → metin):
- `ResultScreen` — kök, tam ekran karartma.
- `Title` — iki varyant: normal iniş "Gidilen mesafe"; çarpma "ÇARPTIN!  Gidilen mesafe" (çarpma varyantında "ÇARPTIN!" vurgulu, kırmızı).
- `Distance` — "312 m" (büyük, ekranın yıldızı; 5 haneye kadar).
- `Gold` — "+14 ALTIN   (Toplam 244)" (altın ikonu; toplam 6 haneye kadar).
- `ReplayButton` — "TEKRAR OYNA" (birincil).

İstenen hâller: normal iniş varyantı ve çarpma varyantı (çarpmada ör. buruşmuş kâğıt, kırmızı damga gibi bir dokunuş); açılış animasyonu (not olarak: kart düşer/yapışır, sayı sayarak artar vb. uGUI'de yapılabilir şekilde).

JSON'da `screen`: `"result_screen"`; varyant farklarını ilgili elementlere `"variants": { "crash": { … } }` alanıyla ekle.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.

---

## 5) Duraklatma menüsü (`pause_menu`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz ekran ekran tasarlanıyor; önceki prompt'ta ortak bir kit (fontlar, paneller, butonlar, ikonlar) tanımlandıysa onu kullan.

**Bu ekran — Duraklatma menüsü:** Oyun sırasında Esc'ye basınca açılır, oyun donar; Esc ya da DEVAM ET ile kapanır. Arkada donmuş oyun sahnesi var, şu an %60 siyahla karartılıyor.

Öğeler (id → metin):
- `PauseMenu` — kök, tam ekran karartma.
- `Box` — ortadaki menü kartı (şu an 680×480; değiştirebilirsin).
- `Title` — "DURAKLATILDI".
- `ResumeButton` — "DEVAM ET" (birincil).
- `MainMenuButton` — "ANA MENÜ" (ikincil; atışı bırakıp giriş ekranına döner).
- `VolumeTitle` — "SES" (hoparlör ikonu eklenebilir).
- `VolumeSlider` — ana ses kaydırıcısı, 0–1; alt öğeleri `Fill` ve `Handle`.
- `VolumeValue` — "%80" (0–100).

İstenen hâller: normal; `ResumeButton` hover; kaydırıcı sürüklenirken handle hâli; açılış animasyonu (not olarak).

JSON'da `screen`: `"pause_menu"`.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.

---

## 6) Mağaza (`shop_menu`)

**Rol:** Sen bir oyun arayüzü (UI/HUD) tasarımcısısın. Unity'ye aktarılabilecek, net ölçülü, stilize arayüzler tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kâğıt bir uçağı kuleden fırlatır, uçak dünyanın ünlü şehirlerinden geçen bir caddenin üstünde süzülür, yıldız toplar; yıldızlar altın kazandırır, altınla mağazadan uçak rengi, şekli ve iz efekti alınır. Arayüz ekran ekran tasarlanıyor; önceki prompt'ta ortak bir kit (fontlar, paneller, butonlar, ikonlar) tanımlandıysa onu kullan.

**Bu ekran — Mağaza:** Giriş ekranındaki MAĞAZA butonuyla açılan tam ekran sayfa. Oyuncu altınla kozmetik alır ve aldıklarından birini kullanır. Üç sekme var; her sekme en fazla 6 eşya gösterir (3 sütun × 2 satır kart ızgarası).

Öğeler (id → metin):
- `ShopMenu` — kök, tam ekran arka plan.
- `Title` — "MAĞAZA".
- `Gold` — sağ üstte "ALTIN: 6105" (altın ikonu; 6 haneye kadar).
- Sekmeler: `TabRENK` "RENK", `TabŞEKİL` "ŞEKİL", `TabEFEKT` "EFEKT" (seçili olan belirgin).
- `Cards` — kart ızgarası kapsayıcısı; içinde her eşya için `Card_<eşya id>`:
  - `Name` — eşya adı.
  - Renk sekmesinde `Swatch` — rengin örneği (kâğıt uçak renginin kâğıda boyanmış bir lekesi gibi olabilir; koyu renkler kartta kaybolmasın).
  - Şekil ve efekt sekmelerinde `Description` — kısa açıklama + `Preview` — önizleme ikonu (aşağıda).
  - `ActionButton` — dört hâl: **satın alınabilir** "SATIN AL · 150" (birincil, altın ikonlu olabilir); **altın yetmiyor** — aynı metin, soluk, fiyat kırmızı, kilit ikonu, tıklanamaz; **sahip** "KULLAN" (ikincil); **kullanılıyor** "KULLANILIYOR" (yeşilimsi onay hâli, ör. damga/tik).
- `BackButton` — "GERİ" (geri oku ikonlu olabilir).

Eşyalar (id — ad — fiyat — açıklama):
- **RENK:** `color_classic` Klasik 0 (#C6B8A1) · `color_sky` Gök Mavisi 150 (#73B8FF) · `color_mint` Nane Yeşili 150 (#80EBAD) · `color_rose` Gül Pembe 200 (#FF8CBF) · `color_night` Gece Siyahı 250 (#292B38) · `color_gold` Altın Sarısı 300 (#FFCC40).
- **ŞEKİL:** `shape_classic` Klasik 0 "Bildiğin kâğıt uçak" · `shape_dart` Ok 400 "Dar ve sivri" · `shape_swallow` Kırlangıç 600 "Çatallı kuyruk" · `shape_glider` Planör 800 "Geniş kanatlar" · `shape_jet` Avcı 1000 "Delta kanat, çift kuyruk" · `shape_manta` Vatoz 1200 "Kısa ve çok geniş".
- **EFEKT:** `effect_classic` Klasik 0 "İnce rüzgar izi" · `effect_rainbow` Gökkuşağı İzi 450 "Renk renk uzun iz" · `effect_gold` Altın İz 600 "Parlayan altın toz".

**Önizleme ikonları (SVG, 160×120, üstten görünüş, krem kâğıt + kırmızı ayrıntı):**
- `preview_shape_classic`: klasik dart; ok ucu gibi üçgen, kanat uçlarının arkasında küçük kırmızı kanatçıklar.
- `preview_shape_dart`: çok dar, iğne gibi uzun ok; kuyruk kenarında ince kırmızı şerit.
- `preview_shape_swallow`: geriye ok açılı kanatlar ve arkada iki uzun sivri uçlu çatallı kuyruk; çatalın iç kenarı kırmızı.
- `preview_shape_glider`: geniş, neredeyse düz kanatlar, küt basamaklı burun, kanat uçlarında kırmızı kanatçıklar.
- `preview_shape_jet`: büyük delta kanat, burnun arkasında küçük kanardlar, arkada iki kırmızı kuyruk.
- `preview_shape_manta`: kısa ve çok geniş baklava/elmas silüeti, önde iki küçük "boynuz", arkada ince kırmızı kuyruk dikeni.
- `preview_effect_classic`: kanat uçlarından iki ince beyaz/gri çizgi.
- `preview_effect_rainbow`: iki kalın, kırmızı→sarı→yeşil→mavi→mor iz.
- `preview_effect_gold`: iki parlak altın iz, etrafında küçük parıltılar.

İstenen hâller: RENK sekmesi açık (6 kartın hepsi; biri "KULLANILIYOR", biri "KULLAN", biri "altın yetmiyor", kalanlar "SATIN AL"); ŞEKİL sekmesi açık (önizleme ikonlarıyla); EFEKT sekmesi açık (3 kart); kart hover hâli.

JSON'da `screen`: `"shop_menu"`. Kart düzenini **bir kez** şablon olarak ver (`"id": "Card_template"` ve altındaki öğeler), ızgara için `"grid": { "columns": 3, "cellSize": [w, h], "spacing": [x, y], "origin": [x, y] }` alanı ekle; dört buton hâlini `ActionButton` için `"variants": { "buy": …, "cantAfford": …, "owned": …, "equipped": … }` ile ver.

### Ortak stil — kâğıt / el işi (bütün ekranlarda aynı)
- **His:** masada kesilip yapıştırılmış kâğıtlardan oluşan bir arayüz. Krem kâğıt kartlar, kraft kâğıt şeritler, köşelerde yarı saydam bant parçaları ya da ataçlar, hafif yırtık veya katlanmış köşeler, kurşun kalem / mürekkep kenar çizgileri. Kartlar hafif eğik durabilir (±3°), ama okunaklılık her şeyden önce gelir.
- **Palet (genişletebilirsin, bunlar sabit kalsın):** krem kâğıt `#F4EBDD`, kraft kâğıt `#C9A27A`, mürekkep `#2B2B3A`, uçak kırmızısı `#B1180B`, altın (para birimi) `#F2B630`, mavi mürekkep `#2F5D9E`.
- **Yazı tipleri:** Google Fonts'tan, **OFL lisanslı** ve **Türkçe karakterleri (ğ Ğ ş Ş ı İ ç Ç ö Ö ü Ü) eksiksiz destekleyen** iki font: başlıklar için el yazısı / marker hissi veren bir font, gövde ve sayılar için okunaklı yuvarlak bir font. Fontların tam adını yaz.
- Oyun 3D bir şehirde geçiyor ve gökyüzü gün batımı tonlarında (indigo → magenta → turuncu); arayüz bu sahnenin önünde okunaklı kalmalı.

### Teknik sınırlar (kesin uyulmalı) — Unity uGUI'ye birebir aktaracağım
- **Ekran:** referans 1920×1080, `ScaleWithScreenSize` (genişlik/yükseklik eşleşmesi 0.5). Düzen 16:9'dan 21:9'a (2560×1080) kadar taşmadan çalışmalı; kenara yapışacak öğeleri uygun köşeye/kenara anchor'la.
- **Kullanılabilecekler:** dikdörtgen görüntüler (sprite, 9-slice olabilir), metin, döndürme, tek bir gölge (renk + piksel ofset), tek dış çizgi, buton durum renkleri/ölçekleri (normal, hover, pressed, disabled), basit animasyonlar (ölçek, solma, kayma, dönme; süre ve eğri ile).
- **Kullanılamayacaklar:** bulanıklık, CSS filtreleri, blend mode'lar, gradyan metin, metinde doku. Bir doku, kenar ya da şekil gerekiyorsa onu **SVG asset** olarak ver (ben sprite'a çevireceğim). SVG'ler sade olsun: path / rect / circle, düz dolgu, metin içermesin, fontsuz.
- **Metinler değişmez:** aşağıda verilen Türkçe metinleri aynen kullan (büyük/küçük harf dahil). Sayılar örnek değerdir, oyunda değişir; en uzun değerde de taşmamalı.
- **Element id'leri değişmez:** aşağıdaki id'ler oyundaki nesne adları; JSON'da aynen kullan. Ek süs öğeleri ekleyebilirsin (id'lerini sen ver, `"decor": true` yaz).

### İstediklerim
1. **Mockup:** 1920×1080 ekran görüntüsü gibi; aşağıda istenen bütün hâller/varyantlar ayrı ayrı.
2. **Not:** kısa tasarım gerekçesi, animasyonlar ve hâl geçişleri.
3. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla:

```json
{
  "screen": "<ekran id>",
  "reference": [1920, 1080],
  "fonts": { "title": "<font adı>", "body": "<font adı>" },
  "colors": { "paper": "#F4EBDD", "kraft": "#C9A27A", "ink": "#2B2B3A", "red": "#B1180B", "gold": "#F2B630", "blueInk": "#2F5D9E" },
  "elements": [
    {
      "id": "StartButton", "type": "button", "parent": "StartScreen",
      "anchor": "center", "pivot": [0.5, 0.5], "pos": [0, -110], "size": [340, 90], "rotation": -2,
      "sprite": "button_primary", "color": "#FFFFFF",
      "text": "BAŞLA", "font": "title", "fontSize": 48, "textColor": "#2B2B3A", "align": "center",
      "shadow": { "color": "#0000004D", "offset": [4, -4] }, "outline": null,
      "states": { "hover": { "scale": 1.05 }, "pressed": { "scale": 0.95, "color": "#E8DCC6" }, "disabled": { "alpha": 0.5 } },
      "animation": "açılış: 0.25 s, ölçek 0.8 → 1, ease-out-back"
    }
  ],
  "assets": [
    { "id": "button_primary", "kind": "sprite", "size": [128, 128], "nineSlice": [32, 32, 32, 32], "svg": "<svg …>…</svg>" }
  ]
}
```

- `anchor`: `center`, `top`, `bottom`, `left`, `right`, `top-left`, `top-right`, `bottom-left`, `bottom-right`, `stretch` (ebeveyni kaplar).
- `pos`: anchor noktasına göre piksel; **x sağa, y yukarı** (Unity gibi; CSS'in tersi). `size`: [genişlik, yükseklik] piksel.
- `type`: `panel`, `text`, `button`, `image`, `icon`, `bar`, `slider`.
- `parent`: elementin içinde durduğu elementin id'si (en dıştaki için ekranın kök id'si).
- Renkler `#RRGGBB` ya da saydamlıkla `#RRGGBBAA`.
- `nineSlice`: [sol, sağ, üst, alt] piksel; esnemesi gereken her panel/buton sprite'ı için ver.
- Bir asset önceki bir prompt'ta (ortak kit) tanımlandıysa onu **aynı id ile** kullan, tekrar SVG yazma.
