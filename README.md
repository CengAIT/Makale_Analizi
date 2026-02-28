### 📚 Makale Graf Analiz Uygulaması
Bu proje, bilimsel makaleler arasındaki karmaşık atıf ilişkilerini analiz etmek,
ağ metriklerini hesaplamak ve büyük veri setlerini etkileşimli bir şekilde görselleştirmek amacıyla geliştirilmiş
C# tabanlı bir masaüstü yazılımıdır.

## 🚀 Öne Çıkan Özellikler
- Özgün Veri İşleme: Hazır serileştirme kütüphaneleri kullanılmadan, 
karakter düzeyinde işleme yapan özel bir JsonDocumentParser ile JSON verileri nesne modeline dönüştürülür.
- Sıfırdan Algoritma İmplementasyonu: Temel graf teorisi algoritmaları "black-box" kütüphaneler yerine C# 
ile özgün olarak kodlanmıştır.
- Gelişmiş Görselleştirme: GDI+ teknolojisi kullanılarak binlerce düğüm içeren ağlarda bile yüksek performanslı, 
titremesiz (Double Buffering) bir deneyim sunulur.
- Etkileşimli Arayüz: Graf üzerinde yakınlaştırma (Zoom), 
kaydırma (Pan) ve düğümlerin üzerine gelindiğinde detaylı bilgi kartları görüntüleme özellikleri mevcuttur.

## 🧠 Analiz Metrikleri
Yazılım, ağın yapısını ve düğümlerin önemini belirlemek için şu algoritmaları kullanır:
- Betweenness Centrality (Brandes Algoritması): Düğümlerin bilgi akışındaki kontrol gücünü ölçer. 
Düğüm boyutları bu skora göre dinamik olarak ölçeklenir.
- K-Core Decomposition: Ağın çekirdek yapısını belirler. 
Düğümler, K-Core değerlerine göre renklendirilir (Merkezde koyu mavi, kenarlarda açık mavi).
- H-Index & H-Core: Makalelerin etki alanını analiz eder. Bir düğüme tıklandığında ilgili makalenin 
H-Core kümesini izole ederek gösteren özel bir pencere açılır.

## 🛠️ Teknik Mimari
Proje, Nesne Yönelimli Programlama (OOP) prensiplerine uygun olarak üç katmanlı bir yapıda tasarlanmıştır:
Katman,Sorumluluklar
Veri Katmanı,Ham JSON verisinin okunması ve Node/Edge modellerine dönüştürülmesi.+1
Analiz Katmanı,"Brandes, K-Core ve H-Index hesaplamalarının yürütülmesi."
Sunum Katmanı,GDI+ tabanlı GraphImager ile görselleştirme ve kullanıcı etkileşimi.+1

## 📊 Matematiksel Temeller
Graf üzerindeki dünya koordinatları ve ekran koordinatları arasındaki dönüşüm şu afin matris formülü ile sağlanmaktadır:

$$P_{world}=\frac{P_{screen}-Offset}{Scale}$$

Betweenness Centrality hesaplamasında ise aşağıdaki formül temel alınmıştır:

$$C_{B}(v)=\sum_{s\ne v\ne t}\frac{\sigma_{st}(v)}{\sigma_{st}}$$
