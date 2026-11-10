# VR Basketbol

Unity VR basketbol oyunu (XR Interaction Toolkit).

## Açmak için
1. **Unity 6000.3.19f1** (Unity 6.3) ile aç. Unity Hub → Add → bu klasörü seç.
2. İlk açılışta Unity paketleri indirip içe aktarır; birkaç dakika sürebilir.
3. `Assets/Scenes/SampleScene` sahnesini aç ve Play'e bas.

## Nasıl oynanır
- VR gözlükle: topu tut, potaya at. Kontrolcü çubuğu ile yürü.
- Gözlük olmadan (XR Device Simulator): WASD ile yürü, fare ile bak.
- **OYUNU BAŞLAT** ile 60 saniyelik tur başlar. Her basket 1 puan.

## Oyun Odası
Odada üç stand var. Her standın arkasında (duvarda / panoda) oyuncuya bakan bir bilgi paneli var: skor, süre, rekor.
Buton yok: **ilk sayıyla 60 saniyelik tur kendiliğinden başlar**, süre bitince skor gösterilir, sonraki sayı yeni turu açar.
- **Basketbol** (batı duvarı): Her basket 1 puan. Basket olunca konfeti + altın halka efekti çıkar.
- **Ördek Avı** (kuzey duvarı): 4 büyük, yavaş ördek. Alt sıra 1, üst sıra 2 puan.
- **Kutu Devirme** (doğu duvarı): 6 büyük, hafif kutu. Masadan düşen her kutu 1 puan, hepsi düşerse +3. Kule kendiliğinden yeniden dizilir.

Güney duvarındaki kapı tutma tuşuyla (grip) açılır/kapanır; arkasında boş bir oda var. Köşede süs bir atari makinesi duruyor.
Atılan toplar birkaç saniye sonra kendi rafına geri döner.

Menüler:
- `Arcade > Odayı Güncelle`: odayı yerinde günceller, elle yapılan tabela/renk değişiklikleri korunur.
- `Arcade > Oyun Odasını Sıfırdan Kur`: `OYUN_ODASI`yı silip baştan kurar (sahnede elle yapılan değişiklikler gider, malzeme renkleri kalır).
