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
Odada üç stand var. Her standın arkasında (duvarda / panoda) oyuncuya bakan küçük bir panel sadece oyunun adını ve skoru gösterir.
Süre ya da buton yok: her sayı otomatik olarak skora eklenir.
- **Basketbol** (batı duvarı): Her basket 1 puan. Basket olunca konfeti + altın halka efekti çıkar.
- **Ördek Avı** (kuzey duvarı): 6 büyük, yavaş ördek. Alt sıra 1, üst sıra 2 puan.
- **Kutu Devirme** (doğu duvarı): 6 büyük, hafif kutu. Masadan düşen her kutu 1 puan, hepsi düşerse +3. Kule kendiliğinden yeniden dizilir.

Güney duvarındaki kapı tutma tuşuyla (grip) açılır/kapanır; arkasında boş bir oda var.
Atılan toplar birkaç saniye sonra kendi rafına geri döner.

Menüler:
- `Arcade > Odayı Güncelle`: sadece son istenen değişiklikleri uygular; elle silinen veya değiştirilen şeylere dokunmaz.
- `Arcade > Oyun Odasını Sıfırdan Kur`: `OYUN_ODASI`yı silip baştan kurar (sahnede elle yapılan değişiklikler gider, malzeme renkleri kalır).
