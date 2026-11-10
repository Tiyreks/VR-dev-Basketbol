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
Odada üç stand var. Her standın yanında küçük bir dünya paneli (skor, süre, rekor) ve **BAŞLAT / BİTİR** butonu bulunur. Butona kontrolcü ışınıyla basılır.
- **Basketbol** (batı duvarı): Her basket 1 puan. Basket olunca konfeti + altın halka efekti çıkar.
- **Ördek Avı** (kuzey duvarı): Raylarda gidip gelen ördeklere top at. Alt sıra 1, orta 2, üst sıra 3 puan.
- **Kutu Devirme** (doğu duvarı): Masadaki kutu kulesine top at. Yere düşen her kutu 1 puan, hepsi düşerse +3. Kule kendiliğinden yeniden dizilir.

Atılan toplar birkaç saniye sonra kendi rafına geri döner.

Oda, `Arcade > Oyun Odasını Kur` menüsüyle sahnede yeniden kurulabilir (eski `OYUN_ODASI` silinip baştan oluşturulur).
