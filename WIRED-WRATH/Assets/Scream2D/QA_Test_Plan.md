# Düşman Sistemleri QA Test Planı

Bu plan, WIRED-WRATH projesindeki düşman birimlerinin mekaniksel doğruluğunu ve oyun dengesini test etmek için oluşturulmuştur.

## 1. Test Edilecek Birimler ve Senaryolar

### [ ] Oculus-Null (Göz)
- **Fonksiyonel Test:** Oyuncuyu görüş alanına girdiğinde tespit ediyor mu?
- **Saldırı Testi:** Mermi (Projectile) fırlatıyor mu ve mermi oyuncuya hasar veriyor mu?
- **Zayıf Nokta Testi:** Sadece `Scream Pulse` (Q) sonrası savunmasız kalıyor mu? (Immunity check)
- **Görüş Hattı (LoS):** Duvar arkasındaki oyuncuyu görmemesi gerekiyor.

### [ ] Hertz-Hound (Tazı)
- **Takip Mekanizması:** Oyuncuyu kovalamaya başlıyor mu?
- **Jamming Sistemi:** Yaklaştığında oyuncunun Dash ve Jump yeteneklerini kilitliyor mu?
- **Görsel Geri Bildirim:** Oyuncu mor renge dönüyor mu?
- **Stun Etkisi:** Scream Pulse ile anında ölüyor mu? (Design requirement)

### [ ] Bit-Fly (Sinek Sürüsü)
- **Swarm Behavior:** Rastgele salınım (wobble) hareketi yapıyor mu?
- **Zincirleme Patlama:** `Logic Virus` (F) birine çarptığında diğerleri de patlıyor mu?
- **Performans:** 10+ sinek yan yana olduğunda sonsuz döngüye girip oyunu donduruyor mu? (Fix verification)

### [ ] Echo-Unit (Hayalet)
- **İz Takibi:** Oyuncunun 1 saniye önceki konumlarını takip ediyor mu?
- **Fizik Testi:** Duvarların içinden geçebiliyor mu? (Trigger check)
- **Hasar Testi:** Temas anında `ScreamMeter` sürekli artıyor mu?
- **Savunma:** Oyuncu `Glitch Step` (Dash) ile içinden geçerse hayalet ölüyor mu?

### [ ] Server-Crab (Tank)
- **Savunma:** Normal saldırılara karşı dokunulmaz mı?
- **Aggro:** Oyuncu yaklaştığında hızlanıyor mu?
- **Memory Anchor:** Havadan vuruş (Ground Pound) kabuğu kırıp öldürüyor mu?

## 2. Riskli Kenar Durumlar (Edge Cases)
- Oyuncu öldüğünde düşmanlar hala saldırmaya devam ediyor mu?
- Aynı anda birden fazla Hertz-Hound jam yaparsa kilitlerin açılmasında sorun oluyor mu?
- Düşmanlar sahne dışına (void) çıkabiliyor mu?
