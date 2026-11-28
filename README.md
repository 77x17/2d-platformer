# Cuội Tìm Trăng

Game platformer 2D tiếng Việt cho Windows x64 và trình duyệt WebGL 2. Cuội mang đèn ông sao qua bốn miền để nhặt lại bốn mảnh trăng và giúp cây đa trở về Cung Trăng. Ngoài chó canh và mèo rình trên cao, Cuội còn gặp cá chép bật khỏi mặt nước và quạ sà xuống từ cành. Có thể đọc dấu hiệu rồi né, hoặc rọi đèn để làm choáng. Ở hồi cuối, Cuội né Cây Đa xông ngang, đọc nhịp quét rễ, nhảy qua sóng đất rồi thắp từng nụ trên thân.

[Cơ chế cốt lõi](#co-che) · [Kiến trúc, mỹ thuật và tối ưu](#ky-thuat) · [Vị trí trong bốn màn](#bon-man) · [Quái và boss](#quai-va-boss) · [Cảnh quay trailer](#quay-trailer)

## Chơi bản Windows và WebGL

**Chơi trên máy khác.** Chép nguyên thư mục `Build`, hoặc giải nén toàn bộ tệp ZIP được gửi. Trên Windows x64, mở `PC/Cuoi.exe`. Để chơi bằng trình duyệt có WebGL 2, nhấp đúp `Web/index.html` trong Chrome hoặc Edge. Giữ `Cuoi_Data` và các DLL cạnh file EXE, giữ thư mục dữ liệu `Web/Build` cạnh `index.html`. Hai cách mở đều chạy cục bộ.

Game luôn mở ở menu chính. Nhấn **Chơi** để chọn một màn đã mở. Vào **Cài đặt** từ Home hoặc menu tạm dừng để đổi phím và khôi phục phím mặc định. Tiến độ mở màn được lưu trên máy sau khi qua từng màn. Khi thua trong một màn, có thể chơi tiếp ở cột đèn gần nhất.

| Màn | Đặc trưng | Phần thưởng |
|---|---|---|
| 1 · Bờ Tre Làng Cuội | Qua mương trên bó tre, né chó mèo, đẩy thùng lên tường đất và hạ cầu đối trọng qua chông. Qua bờ ruộng, ngõ lò gốm và bến vó để trở về sân lễ bên hồ. | Mảnh 1 mở Nhảy đôi |
| 2 · Rừng Trúc | Nhảy đôi qua các bậc cao. Đi đường dưới đến cổng khóa, rồi quay lên giàn tre để gióng hai chuông ở hai đầu. | Mảnh 2 mở Leo vách |
| 3 · Sông Đèn | Leo bờ kè, theo bè qua dòng hoa đăng, đánh thức miếu và thắp ba đèn rước ở bến sông để mở cầu. Ở cống nước cổ, lên bè và bấm E để đi lại giữa hai bến. | Mảnh 3 mở Lướt |
| 4 · Đồi Gốc Đa | Đẩy thùng hạ cầu qua chông, nhảy lên bậc đá rồi tới gốc đa. Né các đòn quét rễ và rọi đèn vào sáu nụ vàng. | Mảnh 4 và đoạn kết |

Trước màn 1, Cây Đa đập vỡ trăng thành bốn mảnh. Mỗi màn giấu mười hạt trăng. Cuội có thể lấy mảnh trăng ngay khi đến đích. Bảng Chọn màn ghi số hạt nhặt được nhiều nhất trong một lượt ở mỗi màn, kể cả khi quay về sớm. Sau trận Cây Đa, bốn mảnh liền lại trên Cung Trăng. [Thiết kế câu đố và hoạt cảnh](Documentation/MOON_CROSSINGS.md)

## Điều khiển

| Phím | Hành động |
|---|---|
| A / D hoặc ← / → | Chạy, đẩy thùng khi chạm thùng |
| Space, W hoặc ↑ | Nhảy. Giữ để nhảy cao, nhả sớm để nhảy thấp. Từ màn 2, bấm lần nữa khi trên không. |
| Giữ hướng vào vách và giữ phím nhảy | Leo từ màn 3. Nhả phím nhảy trong khi vẫn giữ hướng vào vách để bám tại chỗ. Giữ nhảy tiếp để trèo qua mép. |
| S hoặc ↓ | Trên bệ một chiều, nhấn để thả xuống tầng dưới. Trên nền đất, giữ để cúi, thả phím để đứng dậy khi có đủ khoảng trống. |
| Shift hoặc K | Lướt từ màn 4 |
| J, X hoặc F | Rọi đèn. Thanh dưới chân Cuội hiện trong 4 giây hồi đèn. |
| E ở bến cáp | Bè chờ ở bến trái. Bấm E trên bè để sang bờ, hoặc đến trụ đèn ở một trong hai bến để điều bè. Nhảy giữa hành trình để nhặt hạt trăng treo dưới cáp. |
| E | Thắp miếu ở Sông Đèn. Từng nhịp cầu tre sẽ trồi lên từ dưới nước. Rọi đèn gần miếu cũng có tác dụng. |
| Tab | Mở bản đồ cả màn. Bản đồ lớn và bản đồ nhỏ đều dần hiện địa hình cùng hạt trăng còn lại ở những nơi Cuội đã đến trong lượt chơi. Nhấn Tab hoặc Esc để trở lại. |
| R | Trở về cột đèn gần nhất đã thắp, mất một tim. |
| Esc · M | Tạm dừng / tiếp tục · bật / tắt âm thanh |
| ↑ / ↓ và Enter, hoặc chuột | Chọn và kích hoạt nút trên menu |

<a id="co-che"></a>

## Cơ chế cốt lõi

Người chơi nhìn mặt bệ để chọn chỗ đáp, đồng thời nghe và quan sát kẻ địch để quyết định nhảy, né hay rọi đèn. Một lần rọi có thể cứu Cuội khỏi cú lao của chó, nhưng phải chờ 4 giây mới dùng tiếp. Hạt trăng tạo lý do để rời đường chính. Mảnh trăng lớn ở cuối màn đưa câu chuyện sang chặng kế tiếp.

| Tính năng | Luật đang chạy trong game | Cách quan sát khi chơi |
|---|---|---|
| Chạy và đổi hướng | Tốc độ tối đa 6 đơn vị/giây, gia tốc 60, hãm 75 đơn vị/giây². Đầu vào được đọc trong `Update`, vận tốc và tiếp xúc được xử lý ở bước vật lý `FixedUpdate`. | Đảo hướng ở đầu một bệ hẹp. Cuội hãm rồi mới tăng tốc về phía kia. |
| Coyote time và jump buffer | Vừa bước hụt khỏi mép, Cuội còn **0,10 giây** được phép bật nhảy. Nếu bấm nhảy sớm tối đa **0,12 giây** trước khi chạm nền, lệnh được giữ để bật ở lần tiếp đất đó. | Thử ở mép giàn tre màn 1. Một cơ chế dung sai cho cú nhảy muộn, cơ chế kia nhận cú nhảy sớm. |
| Độ cao nhảy và nhịp rơi | Nhảy từ đất với vận tốc dọc 12. Nhả sớm giảm một nửa vận tốc đang đi lên. Trọng lực cơ sở 24, khi rơi nhân 1,6 và tốc độ rơi được giới hạn ở 18 đơn vị/giây. | Chạm nhẹ phím để qua bậc thấp. Giữ phím để lên tầng cao cùng một giàn. |
| Kỹ năng theo tiến trình | Màn 2 mở một lần nhảy thêm trên không (vận tốc 11,5). Từ màn 3, giữ hướng vào vách để bám, giữ thêm phím nhảy để leo. Khi lên đỉnh, game dò mặt đứng được và kiểm tra khoảng trống cho thân Cuội trước khi trèo qua. Từ màn 4, Cuội lướt ngang với tốc độ 16 trong 0,22 giây, hồi 0,9 giây. Một lượt ở trên không dùng tối đa một cú lướt. | Ba thẻ hướng dẫn đầu màn 2–4 dừng thế giới, đưa camera tới vị trí thực hành rồi trả quyền điều khiển. |
| Cúi và thả qua bệ | Trên đất, giữ **S/↓** thu chiều cao vùng va chạm từ 1,55 xuống 1,05 đơn vị và đi ở 27% tốc độ chạy. Nhả phím dưới xà thấp, Cuội đợi đủ chỗ mới đứng. Trên bệ một chiều, mỗi lần nhấn chỉ tạm bỏ va chạm với mặt bệ đang đỡ chân. | Giữ ↓ trên giàn cao sẽ rơi xuống và dừng ở bệ tiếp theo. Muốn rơi tiếp phải nhấn lần nữa. |
| Bệ và vật thể | Bệ gỗ mục rung khoảng 0,7 giây sau khi đứng lên rồi rơi. Bệ nâng, bè và Cuội dùng bước vật lý và nội suy. Khi đứng trên bệ di động, chuyển động của chân được tính tương đối với bệ. Thùng gỗ vừa làm bậc leo vừa làm đối trọng cho bàn cân. | Đứng yên trên bè đang chạy để thấy nhân vật đi cùng sàn. Ở màn 1 có thể so sánh hai cách dùng thùng. |
| Đèn ông sao | Rọi trong bán kính 5 đơn vị, làm choáng quái thường 2 giây, hồi sau 4 giây. Miếu, đèn rước và nụ Cây Đa phản ứng theo trạng thái riêng. Thanh hồi hiện dưới chân Cuội trong lúc chờ. | Rọi lúc cá bật lên hoặc đợi nụ boss mở. Khoảng sáng nhìn thấy quanh đèn và tầm tác động của cú rọi được tính riêng. |
| Máu, cột đèn và chơi lại | Mỗi màn bắt đầu với ba tim. Đòn thường lấy một tim và đẩy lùi. Rơi xuống hố, trúng chông hoặc bấm **R** lấy một tim. Nếu còn tim, Cuội trở lại cột gần nhất đã thắp. Hết tim có thể chọn thử lại từ cột đó với ba tim. | Hồi sinh trong cùng lượt giữ các hạt đã nhặt và vùng bản đồ đã thấy. Mở lại cả màn bắt đầu một lượt mới. |
| Khám phá và lưu | Mỗi màn có 10 hạt tùy chọn. Ngay khi lượt hiện tại vượt kỷ lục của màn, game cập nhật bảng Chọn màn. `PlayerPrefs` lưu kỷ lục, tiến độ mở màn, phím và âm thanh trên máy/trình duyệt đang chơi. | Quay ra Home sau khi tìm thêm hạt để xem kỷ lục. Khi chép Build sang máy khác, người nhận bắt đầu bằng dữ liệu lưu của máy đó. |
| HUD và bản đồ | Góc trái có ba tim và số màn. Góc phải có minimap, số hạt và nút tạm dừng. **Tab** tạm dừng thế giới và mở toàn bản đồ. Sương rút quanh đường Cuội thực sự đi qua trên cả hai trục, mép sương mờ dần. Sao Cuội nhấp nháy. Hạt đã nhặt biến mất khỏi bản đồ. | Đi đường thấp qua một nhánh rồi quay lại leo lên. Phần bản đồ trên cao và hạt tại đó chỉ hiện khi tới gần. |

Thông số trong bảng lấy từ asset đang gắn vào game. Có thể đối chiếu cách nhận lệnh, cách đổi tư thế và luật lưu bằng các tệp sau. [PlayerTuning.asset](Assets/Moonlit/Settings/PlayerTuning.asset) · [Bộ điều khiển](Assets/Moonlit/Scripts/CuoiController.cs) · [Va chạm khi cúi và thả bệ](Assets/Moonlit/Scripts/CuoiController.Stance.cs) · [Tiến độ và hồi sinh](Assets/Moonlit/Scripts/MoonlitGame.cs) · [Sương khám phá](Assets/Moonlit/Scripts/MoonlitMapDiscovery.cs)

<a id="ky-thuat"></a>

## Kiến trúc, mỹ thuật và tối ưu

| Hệ thống | Cách triển khai và tác dụng |
|---|---|
| Phiên chơi và dữ liệu màn | Một `MoonlitGame` tồn tại qua các lần đổi scene bằng `DontDestroyOnLoad`. `MoonlitChapterScene` giữ các mốc đất, bệ, điểm xuất phát, cột đèn, hạt và đích. `MoonlitLevel` đọc chúng để dựng vật thể có va chạm, quái và câu đố của lượt chơi. Khi đổi màn, game vẽ giao diện loading trước rồi dùng `LoadSceneAsync` ở chế độ `Single`. Người làm màn có thể chỉnh vị trí ngay trong Scene View và Play trực tiếp màn đó. [Dữ liệu scene](Assets/Moonlit/Scripts/MoonlitChapterScene.cs) · [Dựng màn](Assets/Moonlit/Scripts/MoonlitLevel.cs) |
| Vật lý và hoạt ảnh Cuội | Phần thân dùng `Rigidbody2D` và `CapsuleCollider2D`. Bộ dò nền/vách lọc theo layer và đọc kết quả vào mảng có sẵn. Hoạt ảnh chạy tiến theo quãng đường Cuội di chuyển so với mặt bệ. Mỗi dáng có mốc đặt bàn chân, nắm tay hoặc treo đèn. Khi Cuội bám yên trên vách, tay giữ nguyên điểm bám. Các bộ dáng gồm 8 dáng chạy, 8 dáng leo, 4 dáng trèo mép, 4 dáng cúi và chu kỳ đứng thở/ngắm đèn. [Pha bước và điểm tiếp xúc](Assets/Moonlit/Scripts/CuoiLocomotion.cs) · [Nguồn sprite](Documentation/CUOI_ANIMATION_CREDITS.md) |
| Chiều sâu của cảnh 2D | Camera trực giao bám mượt. Tốc độ trượt ngang của chân trời, bờ xa, bờ gần và lá tiền cảnh lần lượt là 0,04 / 0,33 / 0,63 / 1,16 so với địa hình. Chân trời dùng một tranh. Mỗi lớp còn lại tái bố trí năm ô quanh camera. Khi Cuội chạy qua cổng làng, lá lướt qua nhanh trong khi đường chân trời gần như đứng yên. [Camera và parallax](Assets/Moonlit/Scripts/MoonlitCamera.cs) |
| Bề mặt và điểm tựa | Mỗi chương có vật liệu đất/đá riêng. Mesh tạo đường viền theo địa hình. Nhiễu Perlin dùng vị trí và chương để tạo biến thể ổn định. Shader lấy mẫu vật liệu theo vị trí để giữ mật độ chi tiết trên bờ dài. Cây và cổng được đặt theo chân đế. Bệ nâng có khoen và dây nối vào mặt sàn. [Mesh và vật liệu](Assets/Moonlit/Scripts/TerrainArt.cs) · [Kiểm tra chân công trình](Assets/Moonlit/Scripts/MoonlitGrounding.cs) · [Dây treo bệ nâng](Assets/Moonlit/Scripts/LiftSuspension.cs) |
| Ánh sáng, nước và hiệu ứng | Một mặt nạ phủ thế giới trước lớp HUD. Bán kính sáng thường là 4,4, khi bùng đèn là 6,4 đơn vị, với mép mềm 1,4. Hệ thống chọn tối đa bốn đèn cảnh gần camera để gửi sang shader. Mặt nước lấy mẫu từ chính dải bờ và đèn hiện tại để dựng phản chiếu. Các tia bụi sau khi tắt được đưa vào hàng đợi để dùng lại ở lần phát kế. Lá và đom đóm cũng được tái bố trí quanh camera. [Đèn gần camera](Assets/Moonlit/Scripts/SceneLanternLight.cs) · [Mặt nạ sáng](Assets/Moonlit/Scripts/LanternDarkness.cs) · [Hiệu ứng và tái sử dụng sprite](Assets/Moonlit/Scripts/MoonlitVisuals.cs) |
| Chi phí bản đồ | Minimap lấy mẫu mỗi 0,05 giây. Mặt nạ sương chỉ xét các ô trong elíp quanh Cuội, bán kính ngang 15 và dọc 6,5 đơn vị. Texture được gửi lại khi dữ liệu khám phá thay đổi. Bản đồ Tab và minimap dùng cùng dữ liệu nên khi một nhánh hiện ở góc màn hình, nó cũng hiện trên bản đồ lớn. [HUD bản đồ](Assets/Moonlit/Scripts/MoonlitHud.Map.cs) · [Vẽ địa hình và biểu tượng](Assets/Moonlit/Scripts/MoonlitMapStrip.cs) |
| Nhạc và phản hồi âm thanh | Sáu bản nhạc nền dành cho Home/câu chuyện, bốn miền và trận Cây Đa. Hai câu nhạc ngắn báo thắng hoặc thua. Hai nguồn phát nhạc chuyển âm lượng dần khi đổi cảnh. Lá trúc, nước sông, bước chân trên gỗ, tiếng quạ và trống rễ giúp nhận ra nơi chốn hoặc đòn sắp tới. Khi phát một số âm báo, game hạ nhạc để tiếng báo dễ nghe. [Điều phối âm thanh](Assets/Moonlit/Scripts/MoonlitAudio.cs) · [Bản phối và bản ký âm](Documentation/FESTIVAL_PIANO.md) |
| Windows và WebGL | Cả hai bản có Home và bốn scene chơi. Bản Web đóng gói dữ liệu nén thành các tệp JavaScript đi kèm. Trang đọc thành Blob trong bộ nhớ rồi giao cho bộ nạp Unity. Cách này cho phép nhấp đúp `index.html` trên Chrome/Edge hoặc đưa nguyên thư mục lên máy chủ tĩnh. [Mẫu trang Web](Assets/WebGLTemplates/Moonlit/index.html) · [Bước đóng gói](Assets/Moonlit/Editor/PortableWebFiles.cs) |

**Nguồn tài nguyên để ghi vào báo cáo.** Các atlas Cuội, chó, mèo, cá, quạ, Cây Đa và tranh phong cảnh được tạo bằng OpenAI ImageGen cho dự án. Phần cắt sprite, đặt điểm tiếp xúc, shader, mesh địa hình, dây cầu, bọt nước, chỉ báo đòn và dàn cảnh được thực hiện trong Unity. Phần piano phối lại từ *Trống quân* và *Đi cấy* với tiếng đàn Salamander Grand Piano của Alexander Holm (CC BY 3.0). Liberation Sans đi kèm theo SIL Open Font License. Danh mục bản quyền phân biệt nguồn hình, nguồn âm thanh và tài liệu tham khảo thiết kế. Các phụ lục có prompt, bản ký âm và liên kết tới nguồn gốc. [Bản quyền và nguồn tư liệu](CREDITS.md) · [Địa hình](Documentation/terrain-sources.json) · [Cá và quạ](Documentation/WETLAND_CREATURE_CREDITS.md) · [Âm thanh trận Cây Đa](Documentation/GUARDIAN_AUDIO_CREDITS.md)

<a id="bon-man"></a>

## Bốn màn và các điểm đáng khám phá

Các giá trị **x** bên dưới là tọa độ ngang trong Unity, tính bằng đơn vị thế giới từ trái sang phải. Trong Editor có thể tìm tên tiếng Anh bằng ô tìm kiếm của Hierarchy. Trong bản chơi, dùng tên khu vực trên HUD, cột đèn và dấu hiệu địa hình. Thời lượng là một lượt QA đã biết đường, có đi qua sáu cột đèn mỗi màn và chỉ nhặt một phần hạt trăng. [Dữ liệu lượt chơi 2.7.0](QA/LanternTrails/campaign.json)

| Màn | Cột đèn theo thứ tự, tọa độ x gần đúng | Mảnh trăng lớn | Lượt QA |
|---|---|---|---|
| 1 · Bờ Tre Làng Cuội | 60, 140, 261, 384, 488, 577 | x 640 | 2 phút 06 giây |
| 2 · Rừng Trúc | 48, 102, 234, 368, 483, 556 | x 630 | 2 phút 21 giây |
| 3 · Sông Đèn | 39, 99, 229, 336, 472, 552 | x 632 | 2 phút 04 giây |
| 4 · Đồi Gốc Đa | 37, 80, 136, 261, 324, 385 | x 391, trước trận Cây Đa | 1 phút 58 giây, gồm trận boss |

### Màn 1 · Bờ Tre Làng Cuội

- **Đầu làng và mương tưới.** Từ cổng làng, đường đất dẫn tới bó tre bắc qua mương. Giàn `Irrigation Lookout` ở x 39–53 là chỗ thử đường trên đầu tiên. Hạt ở x 49, y 6,85 nằm trên giàn cao, bên dưới vẫn có đường để đáp. Sau đó sân đình có chó canh ở x 72 và mèo rình trên mái gần x 110.
- **Hai cách dùng thùng.** Gần x 118, đẩy thùng tới chân tường rồi đứng lên thùng để vượt bờ đất. Tới x 318, đẩy thùng sang bàn cân ở x 326,5. Sàn tre đang treo hạ xuống thành đường qua chông ở x 329–340. Góc quay có cả thùng, bàn cân và sàn sẽ cho thấy quan hệ nguyên nhân – kết quả trong một cảnh.
- **Các đường tìm hạt.** Từ vũng cá chép ở x 243 đi lên giàn phơi lò gốm (`Kiln Drying Gallery`, x 267–286). Tại bến vó hãy tìm lối thấp sát mặt nước (`Fishing Weir Underpass`, x 354–378). Xa hơn là tuyến ngọn cây (`Orchard Crown`, x 440–459). Ba nơi này thay đổi độ cao theo ba cách khác nhau trước khi Cuội về hồ lấy mảnh đầu tiên. [Scene làng](Assets/Moonlit/Scenes/01_Bamboo_Village.unity) · [Ảnh nhánh khám phá](QA/Festival/side-route-1.png)

### Màn 2 · Rừng Trúc

- **Đường cao và đường bảo trì.** Cú nhảy thứ hai giúp lên bệ cao trong rừng. Quạ xuất hiện ở x 154. Tại lò than, đi trên mái (`Charcoal Kiln Eaves`, x 174–194) hoặc xuống hành lang bảo trì (`Kiln Service Run`, x 189–203). Hai hạt ở x gần nhau nhưng cao lần lượt khoảng y 14,2 và y 3,5. Cuối màn còn có chòi canh tre ở x 459–481 và lối bảo trì cống ở x 491–510. Quạ thứ hai ở x 524.
- **Cổng hai chuông.** Đi đường dưới đến sát cổng x 405 để thấy hai đèn báo và lời nhắc cổng khóa. Quay lại lối lên giàn ở x 388, nhảy lên nhánh chia đôi. Đáp lên bàn đạp phía tây (x 376,7, y 14,25) và phía đông (x 392,1, y 14,15), theo thứ tự tùy chọn. Mỗi chuông làm sáng một đèn. Khi đèn thứ hai sáng, cổng xoay mở để Cuội trở lại đường dưới đi tiếp. Đoạn này dùng chính cánh cổng để đặt mục tiêu trước khi người chơi tìm ra đường vòng. [Scene rừng](Assets/Moonlit/Scenes/02_Bamboo_Forest.unity) · [Logic cổng](Assets/Moonlit/Scripts/BambooChimeGate.cs) · [Ảnh hai bàn đạp](QA/FinalPass/forest-chime-stones.png)

### Màn 3 · Sông Đèn

- **Vách bến và chợ cá.** Vách đầu ở x 24 cho người chơi học cách bám, leo và chống tay qua mép. Ngay sau đó có đường bệ cao và lối cầu phao thấp ở x 44–65, với một hạt gần mặt nước tại x 55,5. Gác kho (`Warehouse Attic`, x 175–198) cho một góc nhìn cao. Cá chép ở x 219 đón người chạy sát sông. Tới x 289–290 có thể tìm hạt ở xà mái chợ hoặc ở bến neo ngay phía dưới (`Fish Market Rafters` và `Fish Market Moorings`).
- **Hai cách gọi cầu.** Ở miếu x 108, bấm **E** hoặc rọi đèn để đưa cầu ở x 113–129 lên. Tại bến rước, tới gần rồi rọi lần lượt ba đèn ở x 344, 370 và 395. Sau ngọn thứ ba, bốn nhịp tre tại x 402–418 trồi lên lệch nhịp, lắc nhẹ rồi mới đỡ được Cuội. Có thể quay từ bờ trái để thấy phần cầu trước và sau khi thắp.
- **Bè cáp hai chiều.** Bè chờ ở bến trái x 480,2. Lên bè và bấm **E** để sang bến phải x 501,5. Khi cập bến có thể đi chiều ngược lại. Trụ gọi ở cả hai bờ cho phép gọi bè về khi bè ở bờ đối diện. Khoảng giữa chuyến có hạt trăng ở x 490,85, y 7,05. Nhảy lên lấy hạt và căn để rơi trở lại sàn đang chạy. Gần cuối màn còn có cá bật lên ở x 607. [Scene sông](Assets/Moonlit/Scenes/03_Lantern_River.unity) · [Bè cáp](Assets/Moonlit/Scripts/SluiceFerry.cs) · [Ảnh hạt giữa hành trình](QA/Festival/ferry-moon-mote.png)

### Màn 4 · Đồi Gốc Đa

- **Ghép các kỹ năng.** Sau các bậc rễ thấp ở x 53–68, khoảng chông x 84–94 cho Cuội phối hợp nhảy đôi và lướt. Nhánh `Windbreak Lookout` ở x 174–202 dẫn lên cao. Ba đèn ở x 162, 190 và 219 gọi cầu x 225–241, nhắc lại luật đã học ở Sông Đèn trong địa hình cao hơn.
- **Lối dưới và cầu xoay.** Gần x 269–277 có hang thấp (`Shrine Grotto`). Ở x 308, người chơi gặp quạ trước khi tới thùng đáy đồi. Đẩy thùng từ x 323,5 lên bàn cân x 328,8 để nhịp cầu xoay hạ sang bờ kia. Giàn thợ đá (`Stonecutters Gantry`, x 356–376) giấu hạt cao ở x 366,9, y 13,65. Cột đèn cuối nằm ở x 385.
- **Mảnh trăng và đấu trường.** Chạm mảnh ở x 391 để ghép đủ bốn mảnh, hồi ba tim và kích hoạt trận ở gốc cây x 409. Lúc này Cuội phải dùng đèn trấn an cây. Chỉ sau nụ thứ sáu mới chuyển sang cảnh đoàn tụ. [Scene đồi](Assets/Moonlit/Scenes/04_Banyan_Hill.unity) · [Cầu đối trọng, chông và bến](Assets/Moonlit/Scripts/MoonlitLevel.Crossings.cs) · [Ảnh cầu xoay](QA/FinalPass/hill-drawbridge.png)

<a id="quai-va-boss"></a>

## Quái thường và trận Cây Đa

| Quái | Tín hiệu người chơi nhận được | Cách đọc tình huống |
|---|---|---|
| Chó canh | Tuần tra trên mặt đất. Khi thấy Cuội phía trước trong khoảng 5 đơn vị, chó sủa, hiện dấu chấm than rồi chờ 0,5 giây trước khi lao ngang. | Đến sân đình màn 1 (x 72) từ phía trước mặt chó để ghi trọn tín hiệu. Nhảy qua hoặc rọi đèn trong lúc chó lấy đà. |
| Mèo rình | Ngồi trên cao. Khi phát hiện Cuội, mèo báo trước khoảng 0,45 giây rồi chồm theo cung trong 0,6 giây về vị trí đã nhắm. Sau khi đáp, mèo tuần tra mặt đất. | Ở mái gần x 110 màn 1, tiếp tục di chuyển sau tín hiệu để mèo đáp sau lưng. Các địch mặt đất cũng có thể làm Cuội mất tim khi chạm. |
| Cá chép | Vòng nước và bọt nổi khoảng 0,9 giây rồi cá bật theo cung trong khoảng 1,1 giây và chìm trở lại. | Đứng ở bờ mương x 243 màn 1 hoặc bờ sông x 219 màn 3 để thấy từ vòng nước đầu đến cú đớp. Chờ cá rơi, né khỏi cung hoặc rọi đèn khi cá nổi. |
| Quạ | Quạ kêu và đánh dấu vị trí Cuội khi bắt đầu cảnh báo. Sau khoảng 1,05 giây, nó sà tới vị trí đó rồi bay về cành. | Ở x 154 màn 2, đứng tới khi dấu báo xuất hiện rồi chạy khỏi điểm đã bị nhắm. Cá và quạ chỉ tính một lần trúng cho mỗi lượt tấn công. |

Quãng đường gặp chó đủ rộng để nhảy qua cú lao ngang. Mái nhà kéo mắt người chơi lên chỗ mèo phục sẵn, vòng nước kéo mắt xuống chỗ cá sắp nhảy, còn dấu của quạ khiến người chơi phải rời vị trí đang đứng. [Chó và mèo](Assets/Moonlit/Scripts/ForestCreature.cs) · [Cá và quạ](Assets/Moonlit/Scripts/RiverAndSkyCreature.cs) · [Vị trí đặt quái](Assets/Moonlit/Scripts/MoonlitJourney.cs)

**Cây Đa giữ trăng.** Cành và tán chuyển động lệch nhịp, mắt gỗ chớp, rễ khí sinh lắc còn chân rễ bám đất. Khi cây chuẩn bị ra đòn, chuyển động nghỉ dịu xuống để người chơi nhìn rõ hướng đánh. Vệt ngang báo cú lao. Nhảy hoặc lướt qua. Rễ quét thấp buộc Cuội bật lên, còn cành quét cao chừa khoảng trống sát đất. Khi cây dậm gốc, chạy khỏi dấu va chạm rồi nhảy qua hai làn sóng rễ lan về hai phía. [Chuyển động và dấu báo](Assets/Moonlit/Scripts/BanyanGuardian.Art.cs) · [Ảnh cú dậm gốc](QA/Guardian/strike-Slam.png)

| Nụ cần thắp | Chuỗi đòn ngay trước khi nụ mở |
|---|---|
| 1 | Lao ngang |
| 2 | Dậm gốc, sóng rễ tỏa hai phía |
| 3 | Quét thấp → quét cao |
| 4 | Lao ngang → dậm gốc |
| 5 | Quét thấp → quét cao → quét thấp |
| 6 | Lao ngang → lao ngang → dậm gốc |

Sau mỗi chuỗi, cây mở nụ vàng trong **5,25 giây**. Đưa Cuội vào tầm rồi bấm **J/X/F**. Mỗi hai nụ làm dịu một rễ. Cả trận có ba rễ. Rọi khi vỏ đóng không thắp được nụ, còn bỏ lỡ cửa sổ thì cây lặp lại chuỗi đó. Vì đèn hồi 4 giây, rọi quá sớm có thể khiến người chơi phải thay đổi vị trí và chờ đèn trong lúc nụ đang mở. [Máy trạng thái và chuỗi đòn](Assets/Moonlit/Scripts/BanyanGuardian.cs) · [Thiết kế, sprite và âm báo boss](Documentation/BANYAN_DUEL.md)

<a id="quay-trailer"></a>

## Gợi ý quay trailer 50 giây

Quay gameplay ở khung ngang 16:9, bật âm thanh và giữ HUD trong ít nhất một đoạn để thấy tim, đèn hồi và hạt trăng thực sự thay đổi. Thời gian trong cột đầu là **nhịp dựng gợi ý**, tính từ đầu video. Trong Unity có thể mở trực tiếp scene màn 2–4 rồi nhấn Play để quay cơ chế đã mở của màn đó. Riêng cú đập trăng phải đi từ Home → Chơi → Bờ Tre Làng Cuội. Ở bản phát hành, dùng màn Chọn màn và tiến độ đã mở.

| Vị trí trong trailer | Nơi quay và thao tác | Khung hình hoặc âm thanh nên giữ |
|---|---|---|
| 0–5 giây | **Trăng vỡ.** Bắt đầu màn 1 từ Home, để đoạn mở đầu tự chạy. Trong hoạt cảnh 9 giây, trăng nứt ở giây 2,4–3,55 rồi bốn mảnh bay xuống từ khoảng giây 3,75. | Mở trên trăng còn nguyên. Cắt sau cú đập khi cả bốn mảnh đã tách và còn nhìn thấy hướng bay. [Khung trăng vỡ](QA/Crossings/story-four-pieces.png) |
| 5–12 giây | **Rời làng.** Chạy ngang cổng làng, nhảy từ giàn tưới lên lấy hạt ở x 49. Có thể cắt sang cá ở x 243 ngay lúc vòng nước xuất hiện. | Để cổng và tán lá lần lượt lướt qua Cuội. Giữ tiếng nhặt hạt hoặc cú cá phá mặt nước làm điểm cắt. [Khung giàn tre](QA/Festival/side-route-1.png) · [Khung cá chép](QA/Campaign/encounter-fish.png) |
| 12–20 giây | **Cổng hai chuông.** Ở màn 2, quay lúc tới cổng từ đường dưới, lúc nhảy đôi lên giàn và lúc đáp lên chuông thứ hai. Quay thêm một lượt chạy qua cổng đã mở. | Ghép tiếng chuông thứ hai với cánh cổng xoay. Cho thấy đèn báo đổi trạng thái để người xem hiểu vì sao cổng mở. [Khung chuông tây](QA/CanopyAndMoon/forest-west-chime.png) |
| 20–29 giây | **Cầu trồi trên sông.** Ở màn 3, thắp sẵn hai đèn x 344 và 370. Bắt đầu quay trước khi rọi đèn cuối x 395, rồi tiến tới mép bờ. | Giữ góc nhìn khi từng nhịp tre nổi lên, lắc và nằm ngang. Đợi Cuội đặt chân lên cầu mới cắt. [Khung cầu đang nổi](QA/LanternTrails/procession-bridge-rising.png) |
| 29–36 giây | **Bè cáp.** Bấm E rời bến trái ở màn 3, bật lên nhặt hạt ở x 491 giữa sông rồi đáp lại. | Một cú máy liên tục cho thấy dây cáp, đèn trên bè, nước và việc sàn vẫn di chuyển dưới chân Cuội. [Khung bè và hạt](QA/Festival/ferry-moon-mote.png) |
| 36–46 giây | **Cây Đa phản công.** Chạm mảnh cuối màn 4 ở x 391. Quay cú dậm của chuỗi thứ hai, Cuội nhảy qua sóng và rọi trúng nụ. | Giữ dấu báo trên đất, sóng rễ và nụ sáng trong cùng phân đoạn. Giữ tiếng trống báo và tiếng nụ được thắp. [Báo dậm](QA/Guardian/tell-Slam.png) · [Nụ mở](QA/Guardian/open.png) |
| 46–50 giây | **Khung cuối.** Trailer giới thiệu có thể dừng ở cảnh Cuội giơ đèn đối diện Cây Đa rồi hiện tên game. Nếu cần kể trọn cốt truyện cho báo cáo, thắp nụ thứ sáu để quay hoạt cảnh đoàn tụ dài khoảng 8,2 giây. | Bản kể trọn có thể cắt từ vầng trăng ghép lại sang bóng Cuội và Cây Đa đi lên. [Khung đấu trường](QA/LanternTrails/arena-banyan-reach.png) · [Khung đoàn tụ](QA/FinalPass/story-reunion.png) |

Để minh họa **kỹ thuật** trong phần thuyết trình, có thể quay thêm hai đoạn độc lập. Ở giàn làng màn 1, giữ ↓ trên bệ trên rồi nhả và nhấn lại ở bệ dưới để thấy quy tắc mỗi lần nhấn chỉ xuống một tầng. Tại chợ cá màn 3, mở Tab khi còn ở tuyến thấp, đóng bản đồ, leo lên xà rồi mở lại từ cùng khu vực để thấy mép sương và hạt mới hiện. [Ảnh bản đồ đã khám phá một phần](QA/LanternTrails/village-atlas-reopened.png)

## Project Unity

Để lấy mã nguồn trên máy khác, chạy `git clone https://github.com/77x17/2d-platformer.git`. Trong Unity Hub, chọn **Add project from disk** và trỏ tới thư mục vừa clone, nơi có `Assets`, `Packages` và `ProjectSettings`. Mở bằng Unity **6000.3.16f1** và chờ Unity tải các package, import asset lần đầu. Trong [Assets/Moonlit/Scenes](Assets/Moonlit/Scenes) có scene menu `00_Home` đứng đầu danh sách build. Bốn scene màn chơi theo sau, gồm `01_Bamboo_Village`, `02_Bamboo_Forest`, `03_Lantern_River` và `04_Banyan_Hill`. Mỗi scene lưu địa hình, bệ di chuyển, vị trí xuất phát, cột đèn và mảnh trăng của màn đó. Mở `00_Home` rồi nhấn Play để bắt đầu từ menu. Mở trực tiếp scene màn chơi để thử ngay màn đó. Từ Home, nhấn Chơi rồi chọn Bờ Tre Làng Cuội để xem đoạn trăng vỡ. Trở về màn hình chính sẽ dỡ màn đang chơi và tải lại Home.

Khi Editor đã đóng, chạy `Tools/Build-Game.ps1` từ PowerShell để build. Nếu Editor đang mở, dùng **Assets → Refresh**, đợi biên dịch xong rồi chọn **Moonlit → 2 - Build Windows executable**. Bản Windows xuất vào `Build/PC`. Để dựng WebGL, chọn **Moonlit → 5 - Build WebGL player**. Thư mục `Build/Web` có thể đưa lên máy chủ HTTP tĩnh. Để chơi tại máy, giữ nguyên thư mục `Build/Web` rồi nhấp đúp `index.html` để mở game bằng Chrome hoặc Edge. [Nguồn mỹ thuật, âm thanh và phông chữ](CREDITS.md)

## Kiểm thử

Bản 2.7.0 cho Cuội cúi và thả xuống bệ dưới. Thanh hồi đèn đi theo chân Cuội. Bản đồ lưu vùng đã khám phá trong lượt đang chơi. Khi ngọn đèn thứ ba sáng ở Sông Đèn, từng nhịp cầu nhô lên khỏi mặt nước. [Kiểm thử chuyển động và tương tác](QA/LanternTrails/results.json) · [Kiểm tra bản đồ trong Unity Game View 4K](QA/LanternTrails/map.csv)

Bản 2.6.0 có bốn đường vòng trong mỗi màn, bè cáp ở Sông Đèn và đổi phím trong Cài đặt. [Kiểm tra các tương tác mới](QA/Festival/results.json)

Bản 2.5.1 sửa lỗi bản đồ trống trong Unity Game View. [Ảnh bản đồ màn 1 ở 4K](QA/MapVisibility/village-atlas.png)

Bản 2.5.0 bổ sung bản đồ và kỷ lục hạt trăng. [Kiểm thử bản đồ và lượt chơi lại](QA/MoteRecords/results.json)

Bản 2.4.0 kiểm tra mười hạt trăng trong mỗi màn, hai lối ở cổng trúc, bè hai chiều và chuyển động khi leo vách. [Ảnh và kết quả bản 2.4.0](QA/CanopyAndMoon/VERIFICATION.md)

Bản 2.3 có lượt kiểm scene, chân cây, khoen treo và giao diện Bản quyền bằng `-moonlitFinalQA`, và lượt kiểm bốn câu đố bằng `-moonlitCrossingQA`. [Ảnh và kết quả](QA/FinalPass/VERIFICATION.md)

Trận Cây Đa có một lượt kiểm riêng đi từ đầu Đồi Gốc Đa đến đoạn kết và bài tập đứng yên chịu đòn. Dùng `-moonlitGuardianQA` để chạy lại. [Cách đọc các đòn và nguồn sprite](Documentation/BANYAN_DUEL.md) · [Kiểm thử trận mới](QA/Guardian/VERIFICATION.md)

Bản Windows nhận `-moonlitCampaignQA` để chạy kiểm tra tích hợp và một lượt xuyên bốn màn, hoặc `-moonlitRouteQA` để chỉ chạy tuyến chơi liên tục. Báo cáo và ảnh camera được ghi vào `QA/Campaign` của project khi chạy từ đường dẫn build ở trên. Chế độ QA dùng tiến độ độc lập với bản chơi thường. [Kết quả và giới hạn kiểm thử](QA/Campaign/VERIFICATION.md)
