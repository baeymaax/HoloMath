# HoloMath - 混合實境數學教育系統

HoloMath 是一個基於 Microsoft HoloLens 2 開發的混合實境數學教育應用程式，專為高中數學課程設計，提供沉浸式的 3D 數學視覺化與互動式學習體驗。

## 功能特色

### 核心功能
- **沉浸式 3D 數學視覺化**
  - 立體幾何展開動畫（立方體折疊/展開系統）
  - 弧度與角度轉換互動動畫
  - 正弦波 3D 視覺化
  - 9 種數學曲線生成（螺旋線、李薩如曲線、環面紐結、玫瑰曲線等）

- **互動式教學系統**
  - 85+ 支教學影片涵蓋多個數學主題
  - JSON 驅動的課程單元管理
  - 3D 物件與教學內容整合
  - 影片播放控制（播放/暫停、快進/倒退）

- **考試測驗系統**
  - 支援填空題和選擇題
  - 自動計分與進度追蹤
  - 倒計時功能
  - 成績統計與分析

- **手勢互動**
  - MRTK 手部追蹤
  - 空間映射與物件定位
  - 凝視指標系統

### 課程內容
- 三角函數（弧度、正弦函數、餘弦函數）
- 空間向量
- 立體幾何
- 參數化曲線

## 技術規格

### 開發環境
- **Unity 版本**: Unity 6000.1.8f1 (Unity 6)
- **IDE**: Visual Studio 2022 / JetBrains Rider
- **作業系統**: Windows 10/11 (64-bit)

### 主要套件與框架
- **Mixed Reality Toolkit (MRTK) 2.8.3** - 微軟官方混合實境開發框架
- **Microsoft Mixed Reality OpenXR 1.11.2** - HoloLens 2 支援
- **Unity XR OpenXR 1.14.3** - Unity XR 系統
- **Universal Render Pipeline (URP) 17.1.0** - 渲染管線
- **TextMesh Pro** - 高品質中文字體渲染
- **Cinemachine 3.1.4** - 攝影機控制

### 目標平台
- **主要平台**: Microsoft HoloLens 2
- **開發測試**: Windows PC (Unity Editor)

## 專案結構

```
HoloMath/
├── Assets/
│   ├── HoloMath/                      # 主要專案資產
│   │   ├── Data/                      # 數據資料
│   │   │   ├── Chi_Data/
│   │   │   │   └── math_questions.json    # 題庫配置
│   │   │   └── questions_picture/         # 題目圖片
│   │   ├── Fonts/                     # 字體資源
│   │   ├── Prefab/                    # 預製物件
│   │   ├── Scenes/                    # 場景檔案
│   │   │   ├── Scence_chi/           # Chi 的場景
│   │   │   ├── Scenes_ccc/           # CCC 的場景
│   │   │   └── Scenes_kun/           # Kun 的場景
│   │   ├── Scripts/                   # C# 腳本
│   │   │   ├── Scripts_ccc/          # 數學視覺化腳本
│   │   │   ├── Script_chi/           # 教材系統腳本
│   │   │   └── Scripts_kun/          # 選單、考試腳本
│   │   ├── Video/                     # 教學影片 (85+ 支)
│   │   └── Materials/                 # 材質資源
│   ├── MRTK/                          # Mixed Reality Toolkit
│   └── XR/                            # XR 設定
├── Packages/                          # Unity 套件
└── ProjectSettings/                   # 專案設定
```

## 快速開始

### 環境準備

1. **安裝 Unity Hub 與 Unity 6000.1.8f1**
   - 下載 Unity Hub: https://unity.com/download
   - 安裝 Unity 6000.1.8f1
   - 安裝模組：
     - Universal Windows Platform Build Support
     - Windows Build Support (IL2CPP)

2. **安裝 Visual Studio 2022**
   - 下載: https://visualstudio.microsoft.com/
   - 安裝工作負載：
     - 使用 C++ 的桌面開發
     - 通用 Windows 平台開發
     - 使用 Unity 的遊戲開發

3. **克隆專案**
   ```bash
   git clone <repository-url>
   cd HoloMath
   ```

4. **開啟專案**
   - 啟動 Unity Hub
   - 點擊「開啟」，選擇 HoloMath 資料夾
   - 等待專案載入與編譯

### 在 Unity Editor 中測試

1. 開啟主選單場景：`Assets/HoloMath/Scenes/Scenes_kun/MainMenu.unity`
2. 點擊 Play 按鈕
3. 使用滑鼠模擬手部射線進行互動

### 部署到 HoloLens 2

#### 1. 建置設定

1. 開啟 **File → Build Settings**
2. 切換平台到 **Universal Windows Platform**
   - Target Device: **HoloLens**
   - Architecture: **ARM64**
   - Build Type: **D3D Project**
   - Build and Run on: **USB Device** 或 **Remote Device**
3. 確認場景清單包含：
   - `MainMenu.unity`
   - `scene0805.unity`

#### 2. Player 設定

1. 點擊 **Player Settings**
2. 檢查以下設定：
   - **Product Name**: HoloMath
   - **XR Plugin Management → OpenXR**: 已啟用
   - **Capabilities** (必要權限):
     - Spatial Perception
     - Microphone
     - Internet Client
     - Webcam

#### 3. 建置專案

1. 點擊 **Build**
2. 選擇輸出資料夾（例如：`Builds/UWP`）
3. 等待建置完成

#### 4. 部署應用程式

**方法 A：使用 Visual Studio (推薦)**

1. 開啟建置資料夾中的 `.sln` 檔案
2. 設定：
   - Configuration: **Release**
   - Platform: **ARM64**
   - Target: **Device** (USB) 或 **Remote Machine** (WiFi)
3. 如使用 Remote Machine：
   - 輸入 HoloLens 2 的 IP 位址
   - 驗證模式：Universal (Unencrypted Protocol)
4. 按 **F5** 或點擊「部署」

**方法 B：使用 Device Portal**

1. 在 HoloLens 2 上啟用開發者模式
2. 瀏覽器開啟：`https://[HoloLens-IP]`
3. 登入 Device Portal
4. Views → Apps → Install App
5. 選擇建置的 `.appx` 套件並安裝

## 教學內容管理

### 修改題目與課程

題庫配置檔案位於：`Assets/HoloMath/Data/Chi_Data/math_questions.json`

#### JSON 結構範例

```json
{
  "units": [
    {
      "unitName": "三角函數",
      "unitDescription": "學習弧度、正弦、餘弦等概念",
      "contents": [
        {
          "contentName": "弧度的定義",
          "videoClip": "Video/trigonometry/radian_intro.mp4",
          "threeDObject": "RadianConverter",
          "pictureClip": "",
          "questions": [
            {
              "id": 1,
              "type": "fillInTheBlank",
              "questionText": "1 弧度約等於 ___ 度",
              "correctAnswer": "57.3",
              "options": [],
              "questionImage": ""
            }
          ]
        }
      ]
    }
  ]
}
```

#### 題目類型

1. **填空題** (`fillInTheBlank`)
   ```json
   {
     "type": "fillInTheBlank",
     "questionText": "問題文字",
     "correctAnswer": "答案",
     "options": []
   }
   ```

2. **選擇題** (`multipleChoice`)
   ```json
   {
     "type": "multipleChoice",
     "questionText": "問題文字",
     "correctAnswer": "1",
     "options": ["選項1", "選項2", "選項3", "選項4"]
   }
   ```

### 新增教學影片

1. 將 `.mp4` 檔案放入 `Assets/HoloMath/Video/`
2. 在 Unity 中選擇影片，設定：
   - Video Codec: **H.264**
   - Transcode: 啟用（如需）
3. 在 JSON 的 `videoClip` 欄位填入相對路徑

### 新增 3D 互動物件

1. 將 3D 模型匯入 `Assets/HoloMath/Prefab/`
2. 在教學場景中設置物件
3. 在 JSON 的 `threeDObject` 欄位填入物件名稱
4. `TutorialContentManager_Test` 會自動控制顯示/隱藏

## 核心腳本說明

### 教學系統
- **TutorialContentManager_Test.cs** (1,798 行)
  - 路徑：`Assets/HoloMath/Scripts/Script_chi/整個教材/題目/`
  - 功能：教材管理、題目載入、計分系統

### 主選單系統
- **MainMenuManager.cs** (277 行)
  - 路徑：`Assets/HoloMath/Scripts/Scripts_kun/首頁/`
  - 功能：主選單 UI、場景轉換、音效播放

### 考試系統
- **Exam3DUIController.cs** (737 行)
  - 路徑：`Assets/HoloMath/Scripts/Scripts_kun/考試系統/`
  - 功能：3D 考試界面、倒計時、自動批改

### 3D 互動物件
- **ImprovedCubeTransformSystem.cs** (344 行)
  - 路徑：`Assets/HoloMath/Scripts/Script_chi/整個教材/3D互動物件/沉浸式教學物件/`
  - 功能：立方體展開/折疊動畫系統

- **MathController.cs** (296 行)
  - 路徑：`Assets/HoloMath/Scripts/Scripts_ccc/Core/`
  - 功能：9 種數學曲線生成與視覺化

### 影片控制
- **VideoController.cs** (28 行)
  - 路徑：`Assets/HoloMath/Scripts/Script_chi/整個教材/教學影片播放/`
  - 功能：播放/暫停、快進/倒退

## 常見問題排解

### 建置失敗

**問題**：Unity 建置失敗或報錯

**解決方法**：
- 確認 Unity 版本為 **6000.1.8f1**
- 檢查 UWP 模組是否已安裝
- 清除 `Library` 資料夾後重新開啟專案
- 確認所有場景都已儲存

### HoloLens 無法連接

**問題**：Visual Studio 無法連接到 HoloLens

**解決方法**：
- 確認 HoloLens 和 PC 在同一網路
- 在 HoloLens **設定 → 更新與安全性 → 開發人員專用** 中啟用開發者模式
- 檢查 Device Portal 是否可訪問：`https://[HoloLens-IP]`
- 確認防火牆未封鎖連接

### 中文字型無法顯示

**問題**：UI 或題目中文顯示為方塊

**解決方法**：
- 確認 `TMP_FontAsset` 已設定中文字體
- 檢查腳本中的 `chineseFont` 和 `chineseFontMaterial` 是否已分配
- 字體路徑：`Assets/HoloMath/Fonts/NotoSansMath-Regular SDF.asset`

### 影片無法播放

**問題**：教學影片無法在 HoloLens 上播放

**解決方法**：
- 檢查影片編碼格式（建議使用 **H.264**）
- 確認影片路徑在 JSON 中正確
- 檢查 `VideoPlayer` 組件是否正確綁定
- 影片檔案大小不宜過大（建議 < 50MB）

### MRTK 手勢無法辨識

**問題**：HoloLens 上手勢互動無反應

**解決方法**：
- 確認 MRTK 設定檔已正確配置
- 檢查場景中是否有 `MixedRealityToolkit` 物件
- 確認 HoloLens 的手部追蹤功能已啟用
- 在良好照明環境下使用

## 開發團隊

本專案由三位開發者協作完成：

- **Chi** - 教材系統、3D 互動物件、影片控制
- **CCC** - 數學視覺化、曲線生成、UI 設計
- **Kun** - 主選單、考試系統、計算機功能

## Git 工作流程

### 當前分支
- **主分支**: `main`
- **開發分支**: `CCCHIEN`

### 提交記錄
```
45d32fe - stable3
fe82b0a - stable2
9ccdd90 - Stable
53bb2a0 - stable
```

## 授權

本專案為教育用途開發。

## 聯絡資訊

如有問題或建議，請聯繫開發團隊。

---

**HoloMath** - 讓數學在混合實境中活起來
