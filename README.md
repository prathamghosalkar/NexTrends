# NextTrends Clothing eCommerce Website

NextTrends Clothing is a modern, feature-rich eCommerce platform designed for online retail businesses. It provides users with a seamless shopping experience, integrated payment gateways, and a secure checkout process. Built using the **ASP.NET Core** framework, it implements key functionalities like user authentication, product reviews, order management, and email notifications.

---

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Project](#running-the-project)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- **User Authentication**
  - Secure login and logout system
  - Email verification using OTP (One-Time Password)
  
- **Product Management**
  - Dynamic product catalog
  - Add to cart, remove from cart
  - Product reviews and star ratings
  
- **Order Management**
  - Invoice generation in PDF format sent via email
  - Order history for users
  - Return policy feature for easy product returns
  
- **Shopping Cart**
  - Integrated shopping cart
  - Item quantity update feature
  
- **Responsive UI**
  - Fully responsive layout using **Bootstrap**
  - Cross-browser compatibility
  
- **Email Notifications**
  - OTP-based email verification
  - Order confirmation with invoice (PDF attachment)
  
- **User Reviews & Ratings**
  - Post reviews on purchased items
  - Star ratings for products

---

## Technologies Used

- **Backend:**
  - ASP.NET Core (C#)
  
- **Frontend:**
  - HTML5, CSS3, JavaScript
  - Bootstrap for responsive design
  
- **Database:**
  - SQL Server (or any database of your choice)

- **Email Service:**
  - SMTP Email service for email verification and order confirmation

- **PDF Generation:**
  - Third-party libraries (e.g., **iTextSharp**) for generating PDF invoices

---

## Installation

### Prerequisites
1. **.NET Core SDK** - Make sure .NET Core SDK is installed on your machine.
2. **SQL Server** - Set up a SQL Server instance for the database.
3. **SMTP Server** - You need SMTP credentials for email services (for OTP, order invoices).

### Steps to Install

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/nextrends-clothing.git
