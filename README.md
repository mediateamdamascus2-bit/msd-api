# MSD API — نشر الخادم

GitHub **لا يشغّل** ASP.NET + PostgreSQL على Pages.  
هذا المجلد جاهز للنشر على Render / Railway / Docker من المستودع.

## تشغيل محلي بـ Docker

من جذر المشروع:

```bash
cd deploy/api
docker compose up --build
```

ثم: `http://127.0.0.1:5148/health`

## ربط التطبيق

في إعدادات تطبيق الجوال ضع عنوان الخادم المنشور، مثال:

`https://YOUR-API-HOST`

حسابات تجريبية عند تفعيل البذرة: `admin` / `1234`
