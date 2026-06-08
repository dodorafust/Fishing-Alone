using UnityEngine;
using System.IO; // Gọi thư viện để Unity biết cách đọc/ghi file trên máy tính (Windows/Mac)

// Chữ "static" nghĩa là cỗ máy này luôn tồn tại, không cần gắn vào vật thể nào cả.
public static class SaveSystem
{
    // Hàm này chỉ ra nơi máy tính sẽ giấu file Save của bạn (Thường là trong ổ C:/Users/.../AppData/LocalLow/...)
    private static string GetPath()
    {
        return Application.persistentDataPath + "/savegame.json";
    }

    // LỆNH LƯU GAME
    public static void SaveGame(GameData data)
    {
        // Biến cái hộp dữ liệu thành một đoạn văn bản (định dạng JSON)
        string json = JsonUtility.ToJson(data, true);

        // Ghi đoạn văn bản đó vào ổ cứng máy tính
        File.WriteAllText(GetPath(), json);

        // In ra màn hình Console để mình biết nó đã lưu ở đâu
        Debug.Log("ĐÃ LƯU GAME VÀO Ổ CỨNG TẠI: " + GetPath());
    }

    // LỆNH TẢI GAME (Dùng cho sau này)
    public static GameData LoadGame()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }
        else
        {
            return new GameData();
        }
    }

    // LỆNH XÓA GAME (Dùng khi bấm Trò Chơi Mới)
    public static void DeleteSaveFile()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Đã xóa file Save cũ để bắt đầu trò chơi mới!");
        }
    }
}