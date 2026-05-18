-- Güvenlik Alarmları Tablosu
CREATE TABLE IF NOT EXISTS ALERTS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AlertType TEXT NOT NULL,
    Severity TEXT NOT NULL,
    SourceIp TEXT,
    TargetPort INTEGER,
    Description TEXT,
    Module TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsResolved BOOLEAN DEFAULT 0
);

-- Kritik Sistem Logları Tablosu
CREATE TABLE IF NOT EXISTS SYSTEM_LOGS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EventId INTEGER NOT NULL,
    Provider TEXT,
    Message TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Ağ Trafiği İstatistikleri Tablosu
CREATE TABLE IF NOT EXISTS TRAFFIC_STATS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalPackets INTEGER DEFAULT 0,
    TotalBytes INTEGER DEFAULT 0,
    DroppedPackets INTEGER DEFAULT 0
);

-- Dashboard Kullanıcıları Tablosu
CREATE TABLE IF NOT EXISTS USERS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'READ_ONLY',
    LastLogin DATETIME
);

CREATE TABLE IF NOT EXISTS APP_SETTINGS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingKey TEXT UNIQUE NOT NULL,
    SettingValue TEXT NOT NULL
);

-- Varsayılan olarak 1 hafta ayarını ekleyelim
INSERT OR IGNORE INTO APP_SETTINGS (SettingKey, SettingValue) VALUES ('LogRetentionDays', 'Weekly');
INSERT OR IGNORE INTO APP_SETTINGS (SettingKey, SettingValue) VALUES ('RemoteLogServerUrl', 'https://api.sirketiniz.com/logs/upload');
INSERT OR IGNORE INTO APP_SETTINGS (SettingKey, SettingValue) VALUES ('Theme', 'dark');