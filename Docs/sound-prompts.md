# Ses efektleri ve müzik prompt'ları

Kişisel/ücretsiz kullanım için 2 araç yeterli:

- **ElevenLabs Sound Effects** (ücretsiz plan): 22 efekt + şehir ambiyansı. Prompt'u yapıştır, süreyi tablodaki değere ayarla,
  döngü (loop) seslerinde loop seçeneğini aç, çıkan varyasyonlardan en iyisini **id adıyla** indir.
- **Suno** (ücretsiz plan): 2 müzik, "Instrumental" açık.

Ücretsiz planlar genelde ticari kullanıma kapalıdır ve atıf isteyebilir; oyun satılacak olursa lisansları yeniden gözden geçir.
Gerekirse kesme / ses seviyesi / döngü noktası için **Audacity** (ücretsiz).

## Dosya kuralları

- Biçim: WAV ya da MP3 (Unity ikisini de alır). Efektler mono, ambiyans ve müzik stereo.
- Başta ve sonda sessizlik olmasın; döngü seslerinde birleşme yerinde tık olmasın.
- Dosya adı = id. Yer: `Assets/Resources/Audio/<id>.mp3` ya da `.wav` (oyun sesleri id ile yükler).
- Tema: kâğıt / el işi; sıcak, hafif, oyuncu ama abartısız.

## Kontrol listesi

| ✓ | id | Araç | Süre | Döngü | Ne zaman çalar |
|---|---|---|---|---|---|
| ✅ | `ui_click` | ElevenLabs | 0.1 s | – | Buton tıklama |
| ✅ | `ui_hover` | ElevenLabs | 0.15 s | – | Butonun üstüne gelme |
| ✅ | `ui_tab` | ElevenLabs | 0.3 s | – | Mağaza sekme değişimi |
| ✅ | `ui_open` | ElevenLabs | 0.4 s | – | Mağaza / duraklatma menüsü açılır |
| ✅ | `ui_close` | ElevenLabs | 0.3 s | – | Menü kapanır |
| ✅ | `ui_purchase` | ElevenLabs | 0.8 s | – | Satın alma |
| ✅ | `ui_denied` | ElevenLabs | 0.4 s | – | Altın yetmiyor |
| ✅ | `ui_equip` | ElevenLabs | 0.3 s | – | KULLAN |
| ✅ | `power_charge_loop` | ElevenLabs | 2 s | ✓ | Güç dolarken (incelik güçle artar) |
| ✅ | `power_sweet` | ElevenLabs | 0.2 s | – | Güç %90 bölgesine girer |
| ✅ | `launch_whoosh` | ElevenLabs | 0.6 s | – | Fırlatma |
| ✅ | `star_blue` | ElevenLabs | 0.4 s | – | Mavi yıldız |
| ✅ | `star_yellow` | ElevenLabs | 0.5 s | – | Sarı yıldız |
| ✅ | `star_red` | ElevenLabs | 0.7 s | – | Kırmızı yıldız |
| ✅ | `stall_warning` | ElevenLabs | 1 s | – | Stall'a girince |
| ✅ | `land_slide` | Sentez | 1 s | – | Yere iniş |
| ✅ | `crash_crumple` | Sentez | 0.7 s | – | Araca / binaya çarpma |
| ✅ | `result_card` | Sentez | 0.6 s | – | Sonuç kartı düşer |
| ✅ | `result_stamp` | Sentez | 0.4 s | – | ÇARPTIN damgası |
| ✅ | `coin_tick` | Sentez | 0.08 s | – | Altın sayılırken (art arda) |
| ✅ | `car_horn` | Sentez | 0.5 s | – | Yakından geçen araç (seyrek) |
| ✅ | `music_menu_loop` | Suno | 60–90 s | ✓ | Giriş ekranı ve mağaza |
| ✅ | `music_flight_loop` | Suno | ~90 s | ✓ | Uçuş |

## Prompt'lar

### Arayüz (`ui`)
- **`ui_click`** — `A single soft finger tap on a thick sheet of cream paper, crisp and short, dry, no reverb, game UI click.`
- **`ui_hover`** — `Very quiet, short paper rustle, like a fingertip brushing across a paper card, subtle UI hover sound.`
- **`ui_tab`** — `Quick single page flip of a paper notebook, light and crisp, game menu tab switch.`
- **`ui_open`** — `A paper card sliding quickly across a wooden desk and stopping, soft whoosh, menu opening.`
- **`ui_close`** — `Short paper card sliding away with a soft swish, menu closing, lighter than opening.`
- **`ui_purchase`** — `Two small coins clinking onto paper followed by a soft rubber stamp thump, cheerful purchase confirmation, game shop.`
- **`ui_denied`** — `Soft dull double tap on cardboard, low and muted, gentle 'not allowed' game UI sound, not harsh.`
- **`ui_equip`** — `A metal paper clip snapping onto a sheet of paper, small satisfying click, equip item sound.`

### Fırlatma (`launch`)
- **`power_charge_loop`** — `Seamless loop of a rubber band slowly stretching with a faint rising creak, tension building, light and clean.`
- **`power_sweet`** — `Bright short wooden tick with a tiny chime, signals the perfect moment, game timing cue.`
- **`launch_whoosh`** — `A paper airplane thrown hard by hand, fast swoosh of air with a faint paper flutter, moving away from the listener.`

### Uçuş (`flight`)
- **`star_blue`** — `Light magical sparkle chime, single soft bell note with shimmer, collecting a small star.`
- **`star_yellow`** — `Brighter magical sparkle chime, two rising bell notes with shimmer, collecting a bigger star.`
- **`star_red`** — `Powerful bright sparkle burst, three fast rising bell notes with a whoosh of energy, collecting a rare power star.`
- **`stall_warning`** — `A paper airplane losing lift, paper fluttering and wobbling in the air, a short descending whistle, falling feel.`

### İniş ve çarpma (`end`)
- **`land_slide`** — `A paper airplane touching down and sliding to a stop on rough asphalt, soft paper scrape fading out.`
- **`crash_crumple`** — `A paper airplane hitting a car hard, sharp paper crumple with a dull thud, short and punchy.`

### Sonuç ekranı (`result`)
- **`result_card`** — `A paper card dropped onto a table with a soft slap, followed by a quick piece of tape being pressed down.`
- **`result_stamp`** — `A heavy rubber stamp slammed onto paper, firm thump with a slight ink squish, dramatic but cartoon-light.`
- **`coin_tick`** — `Single tiny coin tick, very short and bright, for counting up gold quickly, plays many times in a row.`

### Ortam (`ambience`)
- **`car_horn`** — `Short friendly car horn honk from a small car passing below, slightly distant, one beep.`

### Müzik (`music`, Suno — Instrumental açık)
- **`music_menu_loop`** — `Calm cozy lo-fi instrumental, ukulele and soft glockenspiel, warm and playful, sunset mood, seamless loop, no vocals.`
- **`music_flight_loop`** — `Light upbeat instrumental, airy pizzicato strings, soft hand claps and glockenspiel, sense of gliding and freedom, seamless loop, no vocals.`

## Oyunda

- 17 ses ElevenLabs ve Suno ile üretildi (rüzgar döngüsü ve şehir ambiyansı sonradan kaldırıldı). "Sentez" yazan 6 ses (iniş, çarpma, sonuç kartı, damga, altın tıkı, korna) kodla
  üretiliyor: Unity menüsü **Tools > Kağıt Uçak > Generate Synth Sounds** (`Assets/Editor/SynthSounds.cs`). Bunlardan birini
  ElevenLabs ile yapmak istersen aynı id ile `Assets/Resources/Audio/` altına koyup `.wav`'ı sil.
- `SoundManager` sesleri çalar (ana ses ayarına uyar, UI sesleri ve müzik duraklatmada da çalar, MP3'lerin başındaki sessizliği kırpar);
  `GameAudio` şarj, stall, iniş/çarpma, korna ve müziği uçağın durumuna göre yönetir.
