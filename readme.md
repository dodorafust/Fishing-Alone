# 🎣 Fishing Alone - 3D Survival Horror Fishing Game

![Unity](https://img.shields.io/badge/Engine-Unity_6000.x_(URP)-black?style=for-the-badge&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp)
![Blender](https://img.shields.io/badge/3D_Art-Blender-F5792A?style=for-the-badge&logo=blender)
![Platform](https://img.shields.io/badge/Platform-PC_(Standalone)-blue?style=for-the-badge)

> **"Câu cá để sống sót, hay trở thành con mồi tiếp theo?"**  
> **Fishing Alone** là một tựa game 3D góc nhìn thứ nhất (First-Person) kết hợp độc đáo giữa sự thư giãn của thể loại mô phỏng câu cá (Fishing Simulation) và bầu không khí căng thẳng, ngạt thở của thể loại sinh tồn kinh dị (Survival Horror) cùng yếu tố nhập vai (RPG).

---

## 📖 Cốt truyện & Bối cảnh (Storyline)

Bạn tỉnh dậy một mình trong căn nhà gỗ trên một hòn đảo biệt lập đầy sương mù. Trên bàn là lá thư của người bố để lại cùng một cuốn sách hướng dẫn sinh tồn. Nhiệm vụ của bạn rất rõ ràng nhưng cũng đầy tuyệt vọng: **Phải câu đủ 6 loài thủy quái cổ đại (God_Sea) thì mới được trở về nhà.** 

Tuy nhiên, bạn không hề cô độc trên hòn đảo này. Mỗi khi màn đêm buông xuống, một thực thể kinh dị dưới đáy giếng sâu sẽ thức tỉnh. Bạn buộc phải câu cá không chỉ để kiếm tiền nâng cấp trang bị, mà còn phải dùng chính những con cá đó làm **cống phẩm hiến tế** trước khi thời gian cạn kiệt...

---

## ✨ Tính năng nổi bật (Key Features)

### 1. 🎣 Cơ chế Câu cá Vật lý & Độ khó Động (Dynamic Fishing Minigame)
* **Tương tác vật lý 3D:** Mô phỏng quỹ đạo quăng mồi, phao nổi trên mặt nước (WaterZone) và hiệu ứng uốn cong cần câu theo thời gian thực (Procedural Rod Bending) dựa trên lực kéo.
* **Minigame Thanh lực (Tension Bar):** Ứng dụng thuật toán nhiễu toán học (`Mathf.PerlinNoise`) để tạo ra hành vi giãy giụa ngẫu nhiên, mượt mà và khó đoán của từng loài cá.
* **Cân bằng RPG:** Độ rộng của vùng an toàn (Safe Zone) và lực giật của cá được nội suy trực tiếp từ **Độ hiếm của cá** (`Common` $\rightarrow$ `Legendary`) và **Chỉ số Ổn định (Stability)** của cần câu.

### 2. 🎒 Hệ thống Kho đồ & Trang bị RPG (Inventory & Equipment)
* **Kéo - Thả thông minh (Drag & Drop):** Giao diện kho đồ trực quan, tự động phân luồng vật phẩm (Cá, Cần câu, Mồi câu) vào các ô chứa chuyên biệt.
* **Cá thể hóa Vật phẩm (Dynamic Cloning):** Mỗi con cá câu lên đều được tạo bản sao độc lập (`Instantiate`), có kích thước (Size) ngẫu nhiên ảnh hưởng trực tiếp đến kích cỡ mô hình 3D trên tay và giá bán thực tế.
* **3 Chỉ số Cần câu:** 
  * `Lure (Nhử)`: Giảm thời gian chờ cá cắn câu.
  * `Luck (May mắn)`: Tăng tỷ lệ câu được cá hiếm và cá kích thước lớn.
  * `Stability (Ổn định)`: Mở rộng thanh an toàn và kìm hãm tốc độ giật của cá trong Minigame.

### 3. 💰 Vòng lặp Kinh tế & Sinh tồn (Survival & Economy Loop)
* **Chu kỳ Ngày/Đêm & Hiến tế:** Áp lực thời gian thực buộc người chơi phải cân nhắc giữa việc mang cá đi bán lấy tiền (Gold) hay ném xuống giếng để làm dịu cơn thịnh nộ của quái vật.
* **Cửa hàng & NPC:** Tương tác với NPC để bán cá, mua mồi câu, nâng cấp sức chứa túi đồ (Mở khóa các ô bị khóa) và cường hóa cấp độ cần câu.
* **Hệ thống Luyện cá (Transmutation) & Bắt mồi sống:** Tương tác với môi trường tự nhiên để bắt côn trùng/động vật làm mồi câu, hoặc sử dụng vòng tròn ma thuật để biến đổi kích thước cá.

---

## 🛠️ Kiến trúc Kỹ thuật (System Architecture)

Dự án được thiết kế và xây dựng dựa trên tư duy **Hệ thống thông tin** và nguyên lý **Lập trình Hướng đối tượng (OOP)**, đảm bảo tính đóng gói, dễ mở rộng và tối ưu hóa bộ nhớ:

* **Kiến trúc Dữ liệu (ScriptableObjects):** Tách biệt hoàn toàn dữ liệu tĩnh (Model) ra khỏi mã nguồn xử lý (Controller). Lớp cơ sở `ItemData` mở rộng ra `FishData`, `RodData`, và `BaitData`, giúp thiết kế hàng trăm vật phẩm mới ngay trên Unity Inspector mà không cần biên dịch lại code.
* **Hệ thống Lưu/Tải (JSON Serialization):** Toàn bộ trạng thái phiên làm việc (Tọa độ Player, Ngày giờ, Tiền tệ, Tiến độ hiến tế và Mảng ID vật phẩm trong kho) được đóng gói vào DTO (`GameData`) và mã hóa thành file `.json` lưu trữ an toàn tại `Application.persistentDataPath`.
* **Trí tuệ Nhân tạo (AI NavMesh):** Quái vật và động vật hoang dã trên đảo hoạt động dựa trên hệ thống `NavMeshAgent`, tự động tuần tra, né tránh chướng ngại vật và truy đuổi người chơi.
* **Đồ họa & Tối ưu hóa:** Sử dụng **Universal Render Pipeline (URP)** kết hợp với hệ thống Prefab hóa tài nguyên 3D và cấu hình va chạm `Continuous Dynamic` giúp ngăn chặn triệt để hiện tượng xuyên thấu vật lý (Tunneling) và rò rỉ bộ nhớ (Memory Leak).

---

## 🎮 Điều khiển (Controls)

| Phím / Thao tác | Chức năng |
| :--- | :--- |
| `W`, `A`, `S`, `D` | Di chuyển nhân vật |
| `Shift` | Chạy nhanh (Sprint) |
| `Space` | Nhảy (Jump) |
| `Chuột Trái` (Giữ / Nhả) | Quăng cần / Điều khiển thanh lực khi giằng co với cá |
| `Chuột Phải` | Thu dây câu |
| `Phím 1 - 9` / `Lăn chuột` | Chuyển đổi vật phẩm trên thanh công cụ (Hotbar) |
| `Tab` | Đóng / Mở Kho đồ & Túi mồi (Kéo thả chuột để trang bị hoặc vứt đồ) |
| `E` | Tương tác (Nhặt đồ, Bắt mồi, Đọc thư/sách, Nói chuyện với NPC, Bán cá) |
| `G` / Kéo ra ngoài UI | Vứt vật phẩm đang cầm trên tay ra môi trường 3D |
| `Esc` | Mở Menu Tạm dừng (Cài đặt độ nhạy chuột, Âm thanh, Lưu & Thoát) |

---

## 📂 Cấu trúc Thư mục (Project Structure)

```text
Fishing-Alone/
├── Assets/
│   ├── Scripts/          # Toàn bộ mã nguồn C# (Controllers, Managers, UI, AI...)
│   ├── GameData/         # Các tệp dữ liệu tĩnh ScriptableObjects (Cá, Cần, Mồi, Shop)
│   ├── Prefabs/          # Các đối tượng 3D đã đóng gói (Cá, Cần câu, Quái vật, UI...)
│   ├── Scenes/           # Chứa MainMenu Scene và Gameplay Scene
│   └── ...               # Tài nguyên 3D Models, Materials, Shaders (URP), Audio
├── Packages/             # Quản lý các gói thư viện của Unity (TextMeshPro, URP, Input...)
├── ProjectSettings/      # Cấu hình hệ thống, Vật lý, Đồ họa và Build Settings
├── .gitignore            # Bộ lọc chuẩn của Unity giúp loại bỏ thư mục rác (Library, Temp...)
└── Fishing.slnx          # Solution file cho Visual Studio / IDE
```

---

## 🚀 Hướng dẫn Cài đặt & Chạy Dự án (Getting Started)

1. **Clone Repository này về máy tính:**
   ```bash
   git clone https://github.com/dodorafust/Fishing-Alone.git
   ```
2. **Mở dự án bằng Unity Hub:**
   * Chọn **Add** $\rightarrow$ **Add project from disk** và trỏ đến thư mục `Fishing-Alone` vừa tải về.
   * Đảm bảo bạn đang cài đặt phiên bản **Unity 6000.x** (hoặc phiên bản tương đương có hỗ trợ Universal Render Pipeline - URP).
3. **Chạy thử nghiệm (Playtest):**
   * Đợi Unity import tài nguyên và biên dịch mã nguồn.
   * Mở Scene tại đường dẫn `Assets/Scenes/MainMenu.unity` và nhấn nút **Play** ở phía trên màn hình Editor để bắt đầu trải nghiệm trọn vẹn luồng game!

---

## 👨‍💻 Tác giả & Thông tin Đồ án (Author)

Dự án được phát triển độc lập nhằm phục vụ cho học phần **Đồ án Liên ngành (Hệ thống Thông tin)**. Toàn bộ quá trình từ lên ý tưởng (GDD), phân tích thiết kế hệ thống (UML), lập trình C# đến tinh chỉnh mô hình 3D đều được thực hiện bởi:

* **Sinh viên thực hiện:** Phan Văn Phát
* **Mã số sinh viên:** 23017073
* **Giảng viên hướng dẫn:** TS. Mai Xuân Tráng
* **GitHub:** [@dodorafust](https://github.com/dodorafust)

---
*Cảm ơn bạn đã ghé thăm dự án **Fishing Alone**! Nếu bạn thấy dự án này thú vị, đừng quên để lại một ⭐ (Star) trên kho lưu trữ nhé!*