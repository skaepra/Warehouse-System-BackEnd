<div align="center">

# 📦 INVENZA — Warehouse Management System

### Backend — ASP.NET Core 8 Web API

نظام Backend لإدارة المستودعات والمخزون والمبيعات.

<br />

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_Web_API-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge\&logo=microsoftsqlserver\&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Authentication-000000?style=for-the-badge\&logo=jsonwebtokens\&logoColor=white)

</div>

---

## 📋 عن المشروع

 **SQL Server** ومتصل بقاعدة بيانات **ASP.NET Core 8 Web API** مبني باستخدام **INVENZA Warehouse Management System** Backend نظام

يوفر REST API لإدارة المنتجات، المخزون، الموظفين، الطلبات، الفواتير، التصنيفات، وعمليات جرد المخزون.

---

## ✨ المميزات

* 🔐 JWT Authentication و Refresh Token
* 👥 صلاحيات حسب الدور: **Manager / Sales / Storekeeper**
* 📦 إدارة المنتجات والمخزون
* 🏷️ إدارة التصنيفات
* 👨‍💼 إدارة الموظفين
* 🛒 إنشاء وإدارة الطلبات
* 📋 جرد المخزون
* 🧾 إنشاء وعرض الفواتير
* 🔄 تحديث المخزون تلقائياً عند إنشاء أو إلغاء الطلب
* 📖 Swagger لتوثيق وتجربة الـAPI

---

## 🛠️ التقنيات

* **ASP.NET Core 8 Web API**
* **C#**
* **Entity Framework Core**
* **SQL Server**
* **JWT Authentication**
* **Swagger / OpenAPI**

---

## 🏗️ Architecture

المشروع منظم بطريقة تفصل مسؤوليات النظام، ويتضمن بشكل أساسي:

```text
Warehouse_System/
├── Controllers/
├── Table/
├── DTOs/
├── Services/
├── Data/
├── Migrations/
├── ResponseDto/
├── Extensions/
├── Program.cs
└── appsettings.json
```

---

# 🚀 تشغيل المشروع

### المتطلبات

تأكد من تثبيت:

* **.NET 8 SDK**
* **SQL Server**
* **Entity Framework Core CLI**

يمكن التأكد من إصدار .NET:

```bash
dotnet --version
```

### 1. تحميل المشروع

```bash
git clone https://github.com/skaepra/Warehouse-System-BackEnd.git
cd <project-folder>
```

### 2. إعداد قاعدة البيانات

قم بتعديل **Connection String** داخل:

```text
appsettings.json
```

بحيث يتوافق مع إعدادات SQL Server لديك.

### 3. إنشاء / تحديث قاعدة البيانات

نفذ:

```bash
dotnet ef database update
```

### 4. تشغيل الـBackend

```bash
dotnet run
```

أو:

```bash
dotnet run --project Warehouse_System
```

بعد تشغيل المشروع سيظهر لك عنوان الـAPI في الـTerminal، مثل:

```text
https://localhost:7156
```

### 5. فتح Swagger

بعد تشغيل الـAPI افتح:

```text
https://localhost:7156/swagger
```

ومن خلال Swagger يمكنك **استعراض وتجربة جميع الـAPI Endpoints**.

---

## 🔗 API الرئيسية

| المجال            | الوظائف                      |
| ----------------- | ---------------------------- |
| 🔐 Authentication | Login, Refresh Token, Logout |
| 👥 Users          | المستخدمين والموظفين         |
| 📦 Products       | المنتجات والمخزون            |
| 🏷️ Categories    | التصنيفات                    |
| 🛒 Orders         | الطلبات                      |
| 🧾 Invoices       | الفواتير                     |
| 📋 Audits         | جرد المخزون                  |

---

## 🔌 Frontend

يتصل هذا الـBackend مع تطبيق **INVENZA Frontend** المبني باستخدام:

**React + TypeScript + Vite + Tailwind CSS + Redux Toolkit + Axios**

---

## 👨‍💻 Developer

**Ahmad Abo Al Shaar**

React • TypeScript • React Native • ASP.NET 8 • SQL Server

---

<div align="center">

### 📦 INVENZA

Warehouse Management System

</div>
