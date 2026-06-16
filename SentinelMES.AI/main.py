import joblib
import pandas as pd
import numpy as np
import shap
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI(
    title="Sentinel-MES AI Engine",
    description="SCADA Anomali Tespiti ve Açıklanabilir Yapay Zeka (XAI) Servisi",
    version="2.0"
)

print("[1] Yapay Zeka Modeli Yükleniyor...")
try:
    model = joblib.load("models/xgboost_scada_model.pkl")
    explainer = shap.TreeExplainer(model)
    print(" Model ve SHAP Explainer başarıyla yüklendi!")
except Exception as e:
    print(f" HATA: Model yüklenemedi. Detay: {e}")

class ScadaTelemetry(BaseModel):
    wind_speed: float
    active_power: float
    theoretical_power: float
    wind_direction: float

@app.get("/")
def read_root():
    return {"status": "AI Motoru Çevrimiçi", "message": "XGBoost Modeli Dinlemede..."}

@app.post("/analyze")
def analyze_telemetry(data: ScadaTelemetry):
    # 1. Gelen Veriyi DataFrame Formatına Çevir
    input_data = pd.DataFrame([{
        "wind_speed": data.wind_speed,
        "active_power": data.active_power,
        "theoretical_power": data.theoretical_power,
        "wind_direction": data.wind_direction
    }])

    # 2. Modeli Çalıştır ve Tahmin Üret
    prediction = model.predict(input_data)[0]
    probability = model.predict_proba(input_data)[0][1] 
    
    is_anomaly = bool(prediction == 1)
    
    #  UNBOUND_LOCAL_ERROR ÇÖZÜMÜ: Başlangıçta varsayılan güvenli değerleri tanımlıyoruz
    explanation = "SİSTEM NORMAL: Değerler operasyonel fizik sınırları içinde."
    shap_out = {"wind_speed": 0.0, "active_power": 0.0, "theoretical_power": 0.0, "wind_direction": 0.0}

    # 3. Anomali Varsa SHAP İşlemlerini Yürüt
    if is_anomaly:
        shap_values = explainer.shap_values(input_data)
        
        # SHAP çıktı formatı List veya Array durumuna göre güvenli çıkarma simülasyonu
        if isinstance(shap_values, list):
            current_shap = shap_values[1][0] if len(shap_values) > 1 else shap_values[0][0]
        else:
            current_shap = shap_values[0] if len(shap_values.shape) > 1 else shap_values

        shap_out = {
            "wind_speed": float(current_shap[0]),
            "active_power": float(current_shap[1]),
            "theoretical_power": float(current_shap[2]),
            "wind_direction": float(current_shap[3])
        }
        
        impacts = np.abs(current_shap)
        feature_names = input_data.columns
        max_impact_idx = np.argmax(impacts)
        suclu_parametre = feature_names[max_impact_idx]
        
        feature_tr = {
            "wind_speed": "Rüzgar Hızı (m/s)",
            "active_power": "Üretilen Güç (kW)",
            "theoretical_power": "Teorik Güç Kapasitesi",
            "wind_direction": "Rüzgar Yönü"
        }
        suclu_tr = feature_tr.get(suclu_parametre, suclu_parametre)
        explanation = f"YAPAY ZEKA TESPİTİ (XAI): %{round(probability*100, 1)} olasılıkla fiziksel manipülasyon (Spoofing) saptandı! '{suclu_tr}' parametresindeki tutarsızlık tespit edildi."

    #  GÜVENLİ VE DİNAMİK GERİ DÖNÜŞ
    return {
        "is_anomaly": is_anomaly,
        "confidence_score": float(probability),
        "xai_explanation": explanation,
        "details": explanation,                 # Web arayüzü doğrudan bunu okuyorsa diye yedek
        "shap_values": shap_out

    }