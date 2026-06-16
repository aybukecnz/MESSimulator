import pandas as pd
import numpy as np
import xgboost as xgb
import joblib # Model serileştirme (Kaydet/Yükle) , Eğittiğim Sentinel-MES modellerini bir kez eğitip kalıcı hale getirmek için.
import warnings # Eğitim sırasında çıkan gereksiz uyarıları gizlemek için.
warnings.filterwarnings('ignore')

print("[1] SCADA verisi yükleniyor...")
# CSV dosyanın tam adını buraya yaz 
df = pd.read_csv("data/wind_turbine_scada.csv")

# Sütun isimlerini API'mizdeki isimlerle eşleştiriyoruz
df = df.rename(columns={
    "Wind Speed (m/s)": "wind_speed",
    "LV ActivePower (kW)": "active_power",
    "Theoretical_Power_Curve (KWh)": "theoretical_power",
    "Wind Direction (°)": "wind_direction"
})

# Sadece ihtiyacımız olan özellikleri (Feature) alalım
features = ['wind_speed', 'active_power', 'theoretical_power', 'wind_direction']
df = df[features]

print("[2] Veri Hazırlığı: Normal çalışma verisi etiketleniyor...")
# Kaggle verisetinin orijinal hali "Normal" çalıştığı için hepsine 0 (Normal) diyoruz.
df['is_anomaly'] = 0 

print("[3] Siber Tehdit Simülasyonu: Modele öğrenmesi için sentetik anomaliler (Saldırılar) enjekte ediliyor...")
# Verinin %5'i kadar sentetik saldırı (Spoofing) üretiyoruz
np.random.seed(42)
anomaly_count = int(len(df) * 0.05)

# Saldırı 1: Düşük Rüzgar, Çok Yüksek Güç (Sensör Zehirlenmesi)
anomaly_df_1 = df.sample(anomaly_count // 2).copy()
anomaly_df_1['wind_speed'] = np.random.uniform(0.0, 3.5, size=len(anomaly_df_1))
anomaly_df_1['active_power'] = np.random.uniform(2000, 3500, size=len(anomaly_df_1))
anomaly_df_1['is_anomaly'] = 1

# Saldırı 2: Yüksek Rüzgar, Sıfır Güç (Fiziksel Sabotaj / İletişim Kesintisi)
anomaly_df_2 = df.sample(anomaly_count // 2).copy()
anomaly_df_2['wind_speed'] = np.random.uniform(12.0, 25.0, size=len(anomaly_df_2))
anomaly_df_2['active_power'] = np.random.uniform(0, 50, size=len(anomaly_df_2))
anomaly_df_2['is_anomaly'] = 1

# Normal ve Saldırı verilerini birleştirip karıştırıyoruz
train_df = pd.concat([df, anomaly_df_1, anomaly_df_2]).sample(frac=1).reset_index(drop=True)

X = train_df[features]
y = train_df['is_anomaly']

print("[4] XGBoost Yapay Zeka Modeli Eğitiliyor (Bu işlem birkaç saniye sürebilir)...")
# Endüstri standardı XGBoost sınıflandırıcısı
model = xgb.XGBClassifier(
    n_estimators=100, 
    max_depth=5, 
    learning_rate=0.1, 
    random_state=42,
    use_label_encoder=False,
    eval_metric='logloss'
)

model.fit(X, y)

print("[5] Model başarıyla eğitildi! Diske kaydediliyor...")
# Eğitilmiş modeli ve özellik isimlerini bir dosya olarak kaydediyoruz
joblib.dump(model, "models/xgboost_scada_model.pkl")

print(" İŞLEM TAMAM! 'xgboost_scada_model.pkl' dosyası oluşturuldu.")