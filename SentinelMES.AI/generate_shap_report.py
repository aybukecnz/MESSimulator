import pandas as pd
import shap
import pickle
import matplotlib.pyplot as plt

# 1. Eğitilmiş Modelimizi Yükleyelim
model_path = "models/xgboost_scada_model.pkl" 
with open(model_path, 'rb') as f:
    model = pickle.load(f)

feature_names = ['Wind_Speed', 'Active_Power', 'Theoretical_Power', 'Wind_Direction']

sample_data = pd.DataFrame({
    'Wind_Speed': [18.5],
    'Active_Power': [0.0],
    'Theoretical_Power': [3200.0],
    'Wind_Direction': [180.0]
})

explainer = shap.TreeExplainer(model)
shap_values = explainer.shap_values(sample_data)

# --- DEĞİŞEN KISIM: WATERFALL YERİNE FORCE PLOT ---
# JS kütüphanesini başlatmadan statik görsel almak için matplotlib=True yapıyoruz
shap.force_plot(explainer.expected_value, 
                shap_values[0], 
                sample_data.iloc[0], 
                feature_names=feature_names, 
                matplotlib=True, 
                show=False)

plt.title("Sentinel-MES XAI: Force Plot Analysis", fontsize=14, fontweight='bold', pad=30)
plt.savefig("shap_force_plot.png", dpi=300, bbox_inches='tight')
plt.show()