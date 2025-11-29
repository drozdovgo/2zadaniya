USE MedicalClinic;
GO



SELECT 'Пользователи' as Таблица, COUNT(*) as Количество FROM [Пользователь]
UNION ALL
SELECT 'Специализации', COUNT(*) FROM [Специализация]
UNION ALL
SELECT 'Врачи', COUNT(*) FROM [Врач]
UNION ALL
SELECT 'Медицинские карты', COUNT(*) FROM [Медицинская_карта]
UNION ALL
SELECT 'Расписание', COUNT(*) FROM [Расписание]
UNION ALL
SELECT 'Записи', COUNT(*) FROM [Запись]
UNION ALL
SELECT 'Отзывы', COUNT(*) FROM [Отзыв];
GO
