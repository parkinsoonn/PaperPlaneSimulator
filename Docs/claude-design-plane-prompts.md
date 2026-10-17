# Claude Design: 5 kağıt uçak şekli

Her bölüm tek başına bir prompt'tur; ayrı ayrı kopyalayıp Claude Design'a yapıştır. Ortak teknik kısım her prompt'ta tekrarlanıyor, böylece hiçbiri diğerine ihtiyaç duymuyor.
Gelen JSON bloğunu `Assets/Resources/Planes/<id>.json` olarak kaydet; mağazada o şekil kendiliğinden çıkar.

| # | id | Ad | Fiyat |
|---|---|---|---|
| 1 | `dart` | Ok | 400 |
| 2 | `swallow` | Kırlangıç | 600 |
| 3 | `glider` | Planör | 800 |
| 4 | `jet` | Avcı | 1000 |
| 5 | `manta` | Vatoz | 1200 |

---

## 1) Ok (`dart`)

**Rol:** Sen bir oyun karakter/obje tasarımcısısın. Low-poly, düz renkli, katlanmış kâğıt görünümlü 3D modeller tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kağıt bir uçağı kuleden fırlatır, uçak şehir caddesinin üstünde süzülür. Mağazadan farklı uçak şekilleri alınabiliyor. Senden bu şekillerden birini istiyorum: **Ok**.

**Tasarım yönü — Ok:** Hız için katlanmış klasik "dart". Çok dar kanatlar (toplam açıklık ~1.1 m), uzun ve iğne gibi sivri burun, kanatlar burundan kuyruğa düz bir çizgiyle geriye açılır. Altında derin, ince bir omurga (keel) var. Kuyruk kenarında ince bir kırmızı şerit (accent). Üstten bakınca uzun dar bir ok ucu silüeti, yandan bakınca belirgin üçgen omurga.

### Teknik sınırlar (kesin uyulmalı)
- **Eksenler (metre):** +Z burun yönü (ileri), +Y yukarı, +X sağ kanat. Model, uçağın kökenine göre (0,0,0) tanımlanır; y = 0 omurganın en alt noktası.
- **Varsayılan uçak (ölçek referansı):** kanat açıklığı 1.68 m (x ±0.84), boy 2.24 m (z −1.12…+1.12), yükseklik 0–0.80 m. Kanat üst yüzü y ≈ 0.43–0.61, omurga y 0–0.44, arkadaki kırmızı kanatçıklar y 0.56–0.80. 8 panel, toplam 64 üçgen.
- **Kutu sınırı:** x −1.0…+1.0, y 0…0.85, z −1.2…+1.3. Bu kutunun dışına çıkma.
- **Yapı:** model, ince düz **panellerden** oluşur (gerçek katlanmış kâğıt gibi). Her panel 3 veya 4 köşeli **düz** bir yüzeydir (4 köşe aynı düzlemde olmalı) ve `thickness` kadar kalınlaştırılır (0.02–0.07 m). 6–14 panel, toplam ≤ 120 üçgen.
- **Stil:** doku yok, sadece düz renk. Keskin kat çizgileri; paneller kat yerlerinde birbirine değsin, boşluk kalmasın.
- **Malzemeler (sadece bu üçü):**
  - `paper`: ana kâğıt yüzleri. Oyunda mağazadan seçilen renge boyanır (varsayılan krem #C6B8A1).
  - `shade`: katların iç yüzleri, alt yüzler, omurga. Oyunda `paper` renginin koyu tonu olur.
  - `accent`: sabit kırmızı #B1180B; şerit, kanatçık gibi küçük vurgular için, az kullan.
- **Simetri:** sadece **sağ yarıyı** (x ≥ 0) ver ve o panellere `"mirror": true` yaz; sol yarıyı oyun aynalar. Tam ortadaki (x = 0) omurga gibi paneller bir kez yazılır, `"mirror": false`.
- **İz noktası:** `wingtip` = sağ kanadın en arka uç noktası (rüzgar izi buradan çıkar), x > 0.
- **Görüş:** oyunda kamera uçağın 7 m arkasında ve 2.5 m üstünde, ileri bakar; uçak ekranda küçük görünür. Silüet ve renk blokları ince detaydan önemli.

### İstediklerim
1. **Konsept panosu:** 3/4 perspektif, üstten, yandan ve önden görünüş; malzemeler renklendirilmiş (paper krem, shade koyu krem, accent kırmızı).
2. **Oyun kamerası görünümü:** uçağın 7 m arkasından ve 2.5 m üstünden, açık mavi gökyüzü önünde.
3. **3D önizleme:** aşağıdaki JSON'dan (aynalama ve kalınlık dahil) çizilen, fareyle döndürülebilir bir önizleme. JSON ile konsept birebir aynı olmalı.
4. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla (doğrudan Unity'ye aktaracağım):

```json
{
  "id": "dart",
  "name": "Ok",
  "wingtip": [0.55, 0.6, -1.1],
  "panels": [
    { "name": "wing_right", "points": [0, 0.45, 1.25,  0.55, 0.6, -1.1,  0, 0.45, -1.1], "thickness": 0.04, "mat": "paper", "mirror": true },
    { "name": "keel", "points": [0, 0.45, 1.25,  0, 0.45, -1.1,  0, 0.05, -0.9], "thickness": 0.03, "mat": "shade", "mirror": false }
  ]
}
```

- `points`: köşelerin **düz listesi** `[x1,y1,z1, x2,y2,z2, x3,y3,z3(, x4,y4,z4)]`; 3 köşe = üçgen, 4 köşe = dörtgen (köşeler kenar sırasıyla, çapraz değil).
- `thickness`: panel kendi düzlemine dik yönde bu kadar kalınlaşır (merkezde kalır).
- `mat`: `"paper"`, `"shade"` veya `"accent"`.
- Yukarıdaki iki panel sadece format örneği; tasarımı sen yap.

---

## 2) Kırlangıç (`swallow`)

**Rol:** Sen bir oyun karakter/obje tasarımcısısın. Low-poly, düz renkli, katlanmış kâğıt görünümlü 3D modeller tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kağıt bir uçağı kuleden fırlatır, uçak şehir caddesinin üstünde süzülür. Mağazadan farklı uçak şekilleri alınabiliyor. Senden bu şekillerden birini istiyorum: **Kırlangıç**.

**Tasarım yönü — Kırlangıç:** Kırlangıç kuşundan esinlenen zarif bir uçak. Belirgin şekilde geriye ok açılı (swept) kanatlar, toplam açıklık ~1.7 m; kanatlar hafif V (dihedral) yapar. En ayırt edici özellik **çatallı kuyruk**: arkada ortadan ayrılan iki uzun, sivri uç (z ≈ −1.2'ye kadar uzanır). Kısa sivri burun, orta derinlikte omurga. Çatalın iç kenarlarında ince kırmızı (accent) şerit. Üstten bakınca kuş silüeti, çatal hemen okunmalı.

### Teknik sınırlar (kesin uyulmalı)
- **Eksenler (metre):** +Z burun yönü (ileri), +Y yukarı, +X sağ kanat. Model, uçağın kökenine göre (0,0,0) tanımlanır; y = 0 omurganın en alt noktası.
- **Varsayılan uçak (ölçek referansı):** kanat açıklığı 1.68 m (x ±0.84), boy 2.24 m (z −1.12…+1.12), yükseklik 0–0.80 m. Kanat üst yüzü y ≈ 0.43–0.61, omurga y 0–0.44, arkadaki kırmızı kanatçıklar y 0.56–0.80. 8 panel, toplam 64 üçgen.
- **Kutu sınırı:** x −1.0…+1.0, y 0…0.85, z −1.2…+1.3. Bu kutunun dışına çıkma.
- **Yapı:** model, ince düz **panellerden** oluşur (gerçek katlanmış kâğıt gibi). Her panel 3 veya 4 köşeli **düz** bir yüzeydir (4 köşe aynı düzlemde olmalı) ve `thickness` kadar kalınlaştırılır (0.02–0.07 m). 6–14 panel, toplam ≤ 120 üçgen.
- **Stil:** doku yok, sadece düz renk. Keskin kat çizgileri; paneller kat yerlerinde birbirine değsin, boşluk kalmasın.
- **Malzemeler (sadece bu üçü):**
  - `paper`: ana kâğıt yüzleri. Oyunda mağazadan seçilen renge boyanır (varsayılan krem #C6B8A1).
  - `shade`: katların iç yüzleri, alt yüzler, omurga. Oyunda `paper` renginin koyu tonu olur.
  - `accent`: sabit kırmızı #B1180B; şerit, kanatçık gibi küçük vurgular için, az kullan.
- **Simetri:** sadece **sağ yarıyı** (x ≥ 0) ver ve o panellere `"mirror": true` yaz; sol yarıyı oyun aynalar. Tam ortadaki (x = 0) omurga gibi paneller bir kez yazılır, `"mirror": false`.
- **İz noktası:** `wingtip` = sağ kanadın en arka uç noktası (rüzgar izi buradan çıkar), x > 0.
- **Görüş:** oyunda kamera uçağın 7 m arkasında ve 2.5 m üstünde, ileri bakar; uçak ekranda küçük görünür. Silüet ve renk blokları ince detaydan önemli.

### İstediklerim
1. **Konsept panosu:** 3/4 perspektif, üstten, yandan ve önden görünüş; malzemeler renklendirilmiş (paper krem, shade koyu krem, accent kırmızı).
2. **Oyun kamerası görünümü:** uçağın 7 m arkasından ve 2.5 m üstünden, açık mavi gökyüzü önünde.
3. **3D önizleme:** aşağıdaki JSON'dan (aynalama ve kalınlık dahil) çizilen, fareyle döndürülebilir bir önizleme. JSON ile konsept birebir aynı olmalı.
4. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla (doğrudan Unity'ye aktaracağım):

```json
{
  "id": "swallow",
  "name": "Kırlangıç",
  "wingtip": [0.85, 0.62, -0.7],
  "panels": [
    { "name": "wing_right", "points": [0, 0.45, 1.0,  0.85, 0.62, -0.7,  0, 0.45, -0.4], "thickness": 0.04, "mat": "paper", "mirror": true },
    { "name": "keel", "points": [0, 0.45, 1.0,  0, 0.45, -0.6,  0, 0.15, -0.4], "thickness": 0.03, "mat": "shade", "mirror": false }
  ]
}
```

- `points`: köşelerin **düz listesi** `[x1,y1,z1, x2,y2,z2, x3,y3,z3(, x4,y4,z4)]`; 3 köşe = üçgen, 4 köşe = dörtgen (köşeler kenar sırasıyla, çapraz değil).
- `thickness`: panel kendi düzlemine dik yönde bu kadar kalınlaşır (merkezde kalır).
- `mat`: `"paper"`, `"shade"` veya `"accent"`.
- Yukarıdaki iki panel sadece format örneği; tasarımı sen yap.

---

## 3) Planör (`glider`)

**Rol:** Sen bir oyun karakter/obje tasarımcısısın. Low-poly, düz renkli, katlanmış kâğıt görünümlü 3D modeller tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kağıt bir uçağı kuleden fırlatır, uçak şehir caddesinin üstünde süzülür. Mağazadan farklı uçak şekilleri alınabiliyor. Senden bu şekillerden birini istiyorum: **Planör**.

**Tasarım yönü — Planör:** Uzun süzülmeler için geniş kanatlı bir kağıt planör. Geniş, neredeyse düz (az ok açılı) kanatlar, toplam açıklık ~1.9 m; kanat uçları ön kenarda yuvarlatılmış gibi kırık köşeli. Burun **küt**: kâğıt birkaç kez katlanıp öne doğru kalınlaşmış, yandan bakınca basamaklı kat görünür (`shade` ile). Kanat uçlarında **yukarı kalkık kırmızı kanatçıklar** (winglet, accent). Sığ, kısa bir omurga. Üstten bakınca geniş bir "T" / planör silüeti.

### Teknik sınırlar (kesin uyulmalı)
- **Eksenler (metre):** +Z burun yönü (ileri), +Y yukarı, +X sağ kanat. Model, uçağın kökenine göre (0,0,0) tanımlanır; y = 0 omurganın en alt noktası.
- **Varsayılan uçak (ölçek referansı):** kanat açıklığı 1.68 m (x ±0.84), boy 2.24 m (z −1.12…+1.12), yükseklik 0–0.80 m. Kanat üst yüzü y ≈ 0.43–0.61, omurga y 0–0.44, arkadaki kırmızı kanatçıklar y 0.56–0.80. 8 panel, toplam 64 üçgen.
- **Kutu sınırı:** x −1.0…+1.0, y 0…0.85, z −1.2…+1.3. Bu kutunun dışına çıkma.
- **Yapı:** model, ince düz **panellerden** oluşur (gerçek katlanmış kâğıt gibi). Her panel 3 veya 4 köşeli **düz** bir yüzeydir (4 köşe aynı düzlemde olmalı) ve `thickness` kadar kalınlaştırılır (0.02–0.07 m). 6–14 panel, toplam ≤ 120 üçgen.
- **Stil:** doku yok, sadece düz renk. Keskin kat çizgileri; paneller kat yerlerinde birbirine değsin, boşluk kalmasın.
- **Malzemeler (sadece bu üçü):**
  - `paper`: ana kâğıt yüzleri. Oyunda mağazadan seçilen renge boyanır (varsayılan krem #C6B8A1).
  - `shade`: katların iç yüzleri, alt yüzler, omurga. Oyunda `paper` renginin koyu tonu olur.
  - `accent`: sabit kırmızı #B1180B; şerit, kanatçık gibi küçük vurgular için, az kullan.
- **Simetri:** sadece **sağ yarıyı** (x ≥ 0) ver ve o panellere `"mirror": true` yaz; sol yarıyı oyun aynalar. Tam ortadaki (x = 0) omurga gibi paneller bir kez yazılır, `"mirror": false`.
- **İz noktası:** `wingtip` = sağ kanadın en arka uç noktası (rüzgar izi buradan çıkar), x > 0.
- **Görüş:** oyunda kamera uçağın 7 m arkasında ve 2.5 m üstünde, ileri bakar; uçak ekranda küçük görünür. Silüet ve renk blokları ince detaydan önemli.

### İstediklerim
1. **Konsept panosu:** 3/4 perspektif, üstten, yandan ve önden görünüş; malzemeler renklendirilmiş (paper krem, shade koyu krem, accent kırmızı).
2. **Oyun kamerası görünümü:** uçağın 7 m arkasından ve 2.5 m üstünden, açık mavi gökyüzü önünde.
3. **3D önizleme:** aşağıdaki JSON'dan (aynalama ve kalınlık dahil) çizilen, fareyle döndürülebilir bir önizleme. JSON ile konsept birebir aynı olmalı.
4. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla (doğrudan Unity'ye aktaracağım):

```json
{
  "id": "glider",
  "name": "Planör",
  "wingtip": [0.95, 0.62, -0.75],
  "panels": [
    { "name": "wing_right", "points": [0, 0.5, 1.05,  0.95, 0.62, 0.0,  0.95, 0.62, -0.75,  0, 0.5, -1.05], "thickness": 0.04, "mat": "paper", "mirror": true },
    { "name": "keel", "points": [0, 0.5, 0.9,  0, 0.5, -1.05,  0, 0.2, -0.85], "thickness": 0.03, "mat": "shade", "mirror": false }
  ]
}
```

- `points`: köşelerin **düz listesi** `[x1,y1,z1, x2,y2,z2, x3,y3,z3(, x4,y4,z4)]`; 3 köşe = üçgen, 4 köşe = dörtgen (köşeler kenar sırasıyla, çapraz değil).
- `thickness`: panel kendi düzlemine dik yönde bu kadar kalınlaşır (merkezde kalır).
- `mat`: `"paper"`, `"shade"` veya `"accent"`.
- Yukarıdaki iki panel sadece format örneği; tasarımı sen yap.

---

## 4) Avcı (`jet`)

**Rol:** Sen bir oyun karakter/obje tasarımcısısın. Low-poly, düz renkli, katlanmış kâğıt görünümlü 3D modeller tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kağıt bir uçağı kuleden fırlatır, uçak şehir caddesinin üstünde süzülür. Mağazadan farklı uçak şekilleri alınabiliyor. Senden bu şekillerden birini istiyorum: **Avcı**.

**Tasarım yönü — Avcı:** Savaş uçağından esinlenen agresif bir kağıt jet. Ana kanat büyük bir **delta** (üçgen), toplam açıklık ~1.6 m, arka kenarı düz. Burnun hemen arkasında küçük **kanard'lar** (ön mini kanatlar). Arkada, gövdenin iki yanında hafif dışa yatık **çift dikey kuyruk** (accent kırmızı). Gövde ortada belirgin bir sırt katı (iki yüzlü çatı gibi) ve sivri burun. Üstten bakınca keskin delta + kanard silüeti, arkadan bakınca iki kırmızı kuyruk.

### Teknik sınırlar (kesin uyulmalı)
- **Eksenler (metre):** +Z burun yönü (ileri), +Y yukarı, +X sağ kanat. Model, uçağın kökenine göre (0,0,0) tanımlanır; y = 0 omurganın en alt noktası.
- **Varsayılan uçak (ölçek referansı):** kanat açıklığı 1.68 m (x ±0.84), boy 2.24 m (z −1.12…+1.12), yükseklik 0–0.80 m. Kanat üst yüzü y ≈ 0.43–0.61, omurga y 0–0.44, arkadaki kırmızı kanatçıklar y 0.56–0.80. 8 panel, toplam 64 üçgen.
- **Kutu sınırı:** x −1.0…+1.0, y 0…0.85, z −1.2…+1.3. Bu kutunun dışına çıkma.
- **Yapı:** model, ince düz **panellerden** oluşur (gerçek katlanmış kâğıt gibi). Her panel 3 veya 4 köşeli **düz** bir yüzeydir (4 köşe aynı düzlemde olmalı) ve `thickness` kadar kalınlaştırılır (0.02–0.07 m). 6–14 panel, toplam ≤ 120 üçgen.
- **Stil:** doku yok, sadece düz renk. Keskin kat çizgileri; paneller kat yerlerinde birbirine değsin, boşluk kalmasın.
- **Malzemeler (sadece bu üçü):**
  - `paper`: ana kâğıt yüzleri. Oyunda mağazadan seçilen renge boyanır (varsayılan krem #C6B8A1).
  - `shade`: katların iç yüzleri, alt yüzler, omurga. Oyunda `paper` renginin koyu tonu olur.
  - `accent`: sabit kırmızı #B1180B; şerit, kanatçık gibi küçük vurgular için, az kullan.
- **Simetri:** sadece **sağ yarıyı** (x ≥ 0) ver ve o panellere `"mirror": true` yaz; sol yarıyı oyun aynalar. Tam ortadaki (x = 0) omurga gibi paneller bir kez yazılır, `"mirror": false`.
- **İz noktası:** `wingtip` = sağ kanadın en arka uç noktası (rüzgar izi buradan çıkar), x > 0.
- **Görüş:** oyunda kamera uçağın 7 m arkasında ve 2.5 m üstünde, ileri bakar; uçak ekranda küçük görünür. Silüet ve renk blokları ince detaydan önemli.

### İstediklerim
1. **Konsept panosu:** 3/4 perspektif, üstten, yandan ve önden görünüş; malzemeler renklendirilmiş (paper krem, shade koyu krem, accent kırmızı).
2. **Oyun kamerası görünümü:** uçağın 7 m arkasından ve 2.5 m üstünden, açık mavi gökyüzü önünde.
3. **3D önizleme:** aşağıdaki JSON'dan (aynalama ve kalınlık dahil) çizilen, fareyle döndürülebilir bir önizleme. JSON ile konsept birebir aynı olmalı.
4. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla (doğrudan Unity'ye aktaracağım):

```json
{
  "id": "jet",
  "name": "Avcı",
  "wingtip": [0.8, 0.5, -1.0],
  "panels": [
    { "name": "delta_right", "points": [0, 0.5, 0.6,  0.8, 0.5, -1.0,  0, 0.5, -1.0], "thickness": 0.04, "mat": "paper", "mirror": true },
    { "name": "tail_fin_right", "points": [0.2, 0.5, -0.6,  0.25, 0.8, -1.0,  0.2, 0.5, -1.0], "thickness": 0.03, "mat": "accent", "mirror": true }
  ]
}
```

- `points`: köşelerin **düz listesi** `[x1,y1,z1, x2,y2,z2, x3,y3,z3(, x4,y4,z4)]`; 3 köşe = üçgen, 4 köşe = dörtgen (köşeler kenar sırasıyla, çapraz değil).
- `thickness`: panel kendi düzlemine dik yönde bu kadar kalınlaşır (merkezde kalır).
- `mat`: `"paper"`, `"shade"` veya `"accent"`.
- Yukarıdaki iki panel sadece format örneği; tasarımı sen yap.

---

## 5) Vatoz (`manta`)

**Rol:** Sen bir oyun karakter/obje tasarımcısısın. Low-poly, düz renkli, katlanmış kâğıt görünümlü 3D modeller tasarlıyorsun.

**Oyun:** "Kağıt Uçak": oyuncu kağıt bir uçağı kuleden fırlatır, uçak şehir caddesinin üstünde süzülür. Mağazadan farklı uçak şekilleri alınabiliyor. Senden bu şekillerden birini istiyorum: **Vatoz**.

**Tasarım yönü — Vatoz:** Manta vatozundan esinlenen, kısa ve çok geniş bir uçan kanat. Boy ~1.8 m, açıklık ~1.9 m; ayrı bir gövde yok, kanat gövdeyle kaynaşık ve ön kenarı yay gibi kavisli (kırık çizgilerle). Önde, burnun iki yanında küçük öne doğru çıkıntılar (vatozun "boynuzları"). Arkada ortadan uzanan ince, uzun bir **kuyruk dikeni** (accent kırmızı). Üst yüz ortada hafif kabarık (sırt katı), alt yüz `shade`. Üstten bakınca baklava/elmas silüeti, uçuşta yumuşak ve geniş durmalı.

### Teknik sınırlar (kesin uyulmalı)
- **Eksenler (metre):** +Z burun yönü (ileri), +Y yukarı, +X sağ kanat. Model, uçağın kökenine göre (0,0,0) tanımlanır; y = 0 omurganın en alt noktası.
- **Varsayılan uçak (ölçek referansı):** kanat açıklığı 1.68 m (x ±0.84), boy 2.24 m (z −1.12…+1.12), yükseklik 0–0.80 m. Kanat üst yüzü y ≈ 0.43–0.61, omurga y 0–0.44, arkadaki kırmızı kanatçıklar y 0.56–0.80. 8 panel, toplam 64 üçgen.
- **Kutu sınırı:** x −1.0…+1.0, y 0…0.85, z −1.2…+1.3. Bu kutunun dışına çıkma.
- **Yapı:** model, ince düz **panellerden** oluşur (gerçek katlanmış kâğıt gibi). Her panel 3 veya 4 köşeli **düz** bir yüzeydir (4 köşe aynı düzlemde olmalı) ve `thickness` kadar kalınlaştırılır (0.02–0.07 m). 6–14 panel, toplam ≤ 120 üçgen.
- **Stil:** doku yok, sadece düz renk. Keskin kat çizgileri; paneller kat yerlerinde birbirine değsin, boşluk kalmasın.
- **Malzemeler (sadece bu üçü):**
  - `paper`: ana kâğıt yüzleri. Oyunda mağazadan seçilen renge boyanır (varsayılan krem #C6B8A1).
  - `shade`: katların iç yüzleri, alt yüzler, omurga. Oyunda `paper` renginin koyu tonu olur.
  - `accent`: sabit kırmızı #B1180B; şerit, kanatçık gibi küçük vurgular için, az kullan.
- **Simetri:** sadece **sağ yarıyı** (x ≥ 0) ver ve o panellere `"mirror": true` yaz; sol yarıyı oyun aynalar. Tam ortadaki (x = 0) omurga gibi paneller bir kez yazılır, `"mirror": false`.
- **İz noktası:** `wingtip` = sağ kanadın en arka uç noktası (rüzgar izi buradan çıkar), x > 0.
- **Görüş:** oyunda kamera uçağın 7 m arkasında ve 2.5 m üstünde, ileri bakar; uçak ekranda küçük görünür. Silüet ve renk blokları ince detaydan önemli.

### İstediklerim
1. **Konsept panosu:** 3/4 perspektif, üstten, yandan ve önden görünüş; malzemeler renklendirilmiş (paper krem, shade koyu krem, accent kırmızı).
2. **Oyun kamerası görünümü:** uçağın 7 m arkasından ve 2.5 m üstünden, açık mavi gökyüzü önünde.
3. **3D önizleme:** aşağıdaki JSON'dan (aynalama ve kalınlık dahil) çizilen, fareyle döndürülebilir bir önizleme. JSON ile konsept birebir aynı olmalı.
4. En sonda **tek bir JSON bloğu**, tam olarak bu şemayla (doğrudan Unity'ye aktaracağım):

```json
{
  "id": "manta",
  "name": "Vatoz",
  "wingtip": [0.95, 0.55, -0.3],
  "panels": [
    { "name": "wing_right", "points": [0, 0.55, 0.9,  0.95, 0.55, -0.3,  0, 0.55, -0.9], "thickness": 0.04, "mat": "paper", "mirror": true },
    { "name": "tail_spike", "points": [0, 0.55, -0.9,  0.03, 0.55, -0.9,  0, 0.55, -1.2], "thickness": 0.02, "mat": "accent", "mirror": true }
  ]
}
```

- `points`: köşelerin **düz listesi** `[x1,y1,z1, x2,y2,z2, x3,y3,z3(, x4,y4,z4)]`; 3 köşe = üçgen, 4 köşe = dörtgen (köşeler kenar sırasıyla, çapraz değil).
- `thickness`: panel kendi düzlemine dik yönde bu kadar kalınlaşır (merkezde kalır).
- `mat`: `"paper"`, `"shade"` veya `"accent"`.
- Yukarıdaki iki panel sadece format örneği; tasarımı sen yap.
