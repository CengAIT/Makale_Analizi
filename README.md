## 📚 Makale Graf Analiz Uygulaması
Bu proje, bilimsel makaleler arasındaki karmaşık atıf ilişkilerini analiz etmek, ağ metriklerini hesaplamak ve büyük veri setlerini özgün algoritmalarla görselleştirmek amacıyla geliştirilmiş bir masaüstü yazılımıdır.

### 🛠️ Yöntem ve Algoritma (Methodology)

* **Veri Modelleme ve Nesne Dönüşümü:**
Dış bağımlılığı en aza indirmek için hazır kütüphane yerine karakter düzeyinde işleme yapan `JsonDocumentParser` ile ham JSON verileri `ArticleDocument` nesnelerine dönüştürülür.
* **Brandes Algoritması ile Merkezilik:** Her düğüm için BFS çalıştırılarak en kısa yol tabanlı Betweenness Centrality (Arasılık Merkeziliği) hesaplanır.
  
  * Skor Fonksiyonu: $C_{B}(v)=\sum_{s\ne v\ne t}\frac{\sigma_{st}(v)}{\sigma_{st}}$ formülü ile ağdaki bilgi akışını kontrol eden düğümler tespit edilir.
    
* **K-Core Decomposition:** Ağdaki önemsiz düğümler elenerek çekirdek yapı ortaya çıkarılır. Düğümler, hesaplanan K değerine göre açık maviden koyu maviye doğru renklendirilir.
* **H-Index Analizi:** Belirli bir makalenin etki alanını ölçmek için atıf yapan komşular üzerinden dinamik H-Index ve H-Core hesaplaması yapılır.

### 📚 Kullanılan Teknolojiler
* **Dil:** C# (.NET Framework) 
* **Görselleştirme:** GDI+ (System.Drawing) & Çift Tamponlama (Double Buffering)
* **Algoritma:** Brandes, K-Core, H-Index (Sıfırdan Implementasyon)
* **Geliştirme Ortamı:** Visual Studio

### 📊 Deneysel Sonuçlar
* **Görselleştirme Performansı:** GDI+ kullanılarak geliştirilen özgün çizim motoru, binlerce düğüm içeren ağlarda bile titremesiz ve yüksek performanslı bir kullanıcı deneyimi sunmaktadır.
* **Koordinat Dönüşümü:** Kullanıcılar $0.12$ ile $10$ kat arasında yakınlaştırma (Zoom) ve kaydırma (Pan) yaparak grafın derinliklerini inceleyebilmektedir.
  
  * **Dönüşüm Formülü:** $P_{world}=\frac{P_{screen}-Offset}{Scale}$
    
* **İstatistiksel Çıktılar:** Uygulama; toplam makale/atıf sayılarını, en çok atıf alan (In-Degree) ve en çok atıf veren (Out-Degree) makaleleri anlık olarak raporlamaktadır.

Proje, akademik atıf ağlarını içeren `data.json` dosyası ile test edilmiştir.

### 🏁 Sonuç
Bu çalışma, bilgisayar mühendisliği ve veri bilimi için kritik öneme sahip graf analizi tekniklerini, dış kütüphanelere bağımlı kalmadan "black-box" mantığından uzak bir şekilde somutlaştırmıştır. Proje, karmaşık verilerin matematiksel modellerle nasıl anlamlı görsel çıktılara dönüştüğünü kanıtlamaktadır.

## 📄 Proje Raporu
Projenin detaylı analizine, algoritma akış şemasına ve deneysel sonuçlarına [buradan](./Makale_Analizi_Rapor.pdf) ulaşabilirsiniz.

## 👥 Katkıda Bulunanlar
Bu proje, Kocaeli Üniversitesi Programlama Laboratuvarı dersi kapsamında geliştirilmiştir.

[Ahsen İkbal TÜRK](https://github.com/CengAIT)

[Zehra GÜLMÜŞ](https://github.com/zehra-ceng)
