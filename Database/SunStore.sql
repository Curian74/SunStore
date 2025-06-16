USE [master]
GO
alter database [SunStore] set single_user with rollback immediate

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'SunStore')
	DROP DATABASE SunStore
GO

CREATE DATABASE SunStore
GO

USE SunStore
GO

CREATE TABLE Users (
	ID INT IDENTITY(1,1) PRIMARY KEY,
	FullName NVARCHAR(100) NOT NULL,
	Username NVARCHAR(100) NOT NULL,
	Password NVARCHAR(100) NOT NULL,
	Address NVARCHAR(200),
	BirthDate Date NOT NULL,
	Email NVARCHAR(100),
    PhoneNumber NVARCHAR(11) NOT NULL,
	Role INT NOT NULL,
	VertificationCode NVARCHAR(200),
	IsBanned INT
)
GO

CREATE TABLE Employee (
	UserID INT PRIMARY KEY,
	Salary FLOAT NOT NULL,
	HiredDate Date NOT NULL,
	Status NVARCHAR(100) NOT NULL,
	FOREIGN KEY (UserID) REFERENCES Users(ID) on delete cascade
)
GO

CREATE TABLE Customer (
	UserID INT PRIMARY KEY,
	Ranking NVARCHAR(10) NOT NULL,
	FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE
)
GO

CREATE TABLE Category(
	ID INT Identity(1,1) PRIMARY KEY,
	Name NVARCHAR(255) NOT NULL
)
GO

CREATE TABLE Product(
	ID INT Identity(1,1) PRIMARY KEY,
	Name NVARCHAR(255) NOT NULL,
	Image VARCHAR(255),
	Description NVARCHAR(4000),
	ReleaseDate Date,
	CategoryID INT NOT NULL,
	[isDeleted] [bit] NULL,
	FOREIGN KEY (CategoryID) REFERENCES Category(ID) ON DELETE CASCADE
)
GO

CREATE TABLE ProductOption(
	ID INT Identity(1,1) PRIMARY KEY,
	Size NVARCHAR(10),
	Quantity INT,
	Price FLOAT,
	Rating FLOAT,
	Discount INT,
	ProductID INT NOT NULL,
	FOREIGN KEY (ProductID) REFERENCES Product(ID) ON DELETE CASCADE
)
GO

CREATE TABLE Vouchers(
	VoucherID INT Identity(1,1) PRIMARY KEY,
	Code VARCHAR(255),
	VPercent INT,
	Quantity INT,
	StartDate datetime,
	EndDate datetime,
	UserID INT,
)
GO

CREATE TABLE Orders(
	ID INT Identity(1,1) PRIMARY KEY,
	ShipperID INT,
	VoucherID INT,
	DateTime DATETIME,
	AdrDelivery NVARCHAR(300),
	PhoneNumber NVARCHAR(100),
	Note NVARCHAR(2000),
	TotalPrice FLOAT,
	Payment VARCHAR(10),
	Status NVARCHAR(100),
	DenyReason NVARCHAR(2000),
	FOREIGN KEY (ShipperID) REFERENCES Employee(UserID),
	FOREIGN KEY (VoucherID) REFERENCES Vouchers(VoucherID) ON DELETE CASCADE
)
GO

CREATE TABLE OrderItem(
	ID INT Identity(1,1) PRIMARY KEY,
	ProductID INT,
	CustomerID INT,
	OrderID INT,
	Quantity INT,
	Price FLOAT,
	FOREIGN KEY (CustomerID) REFERENCES Customer(UserID) ON DELETE CASCADE,
	FOREIGN KEY (ProductID) REFERENCES ProductOption(ID) ON DELETE CASCADE,
	FOREIGN KEY (OrderID) REFERENCES Orders(ID) ON DELETE CASCADE
)
GO

-----INSERT DATA-----
--Users
INSERT INTO Users(FullName,UserName,Password,Address,BirthDate,Email,PhoneNumber,Role, VertificationCode, IsBanned)--*Role: 1_admin,2_cus,3_shipper
VALUES
	(N'Phạm Mạnh Hùng',N'hung123',N'123456','Ha Noi','2004-01-08','hung080104@gmail.com','0975861472',2, '', 0),  --*Role: 1_admin,2_cus,3_shipper
	(N'Trịnh Quốc Bảo',N'admin',N'123456','Ha Noi','2004-05-12','admin.sunstore@gmail.com','0926553412',1, '', 0),
	(N'Chử Hồng Phúc',N'hongphuc',N'123456','Ha Noi','2004-05-12','phuc@gmail.com','0945673661',3, '', 0),
	(N'Lê Trường Sơn',N'sonle123',N'123456','81 QL21','2004-11-12','sonle@gmail.com','0966882113',3, '', 0),
	(N'Nguyễn Thị Hoa', N'nguyenthihoa', N'passwordhoa', N'456 Nguyen Trai', '2002-07-15', 'hungpmhe181830@fpt.edu.vn', '0973332441', 2, '', 0),
    (N'Hoàng Văn Đan', N'hoangvandan', N'passworddan', 'Ha Noi', '2001-06-30', 'minhdangtai2422004@gmail.com', '0934567890', 2, '', 0),
    (N'Lê Thị Lan', N'lethilan', N'passwordlan', N'789 Tran Hung Dao', '2000-05-25', 'lethilan@gmail.com', '0945678901', 3, '', 1),
    (N'Phạm Văn Minh', N'phamvanminh', N'passwordminh', N'12 Pham Ngoc Thach', '1999-04-01', 'phamvanminh@gmail.com', '0956789012', 3, '', 0),
    (N'Nguyễn Văn Dũng', N'nguyenvandung', N'passworddung', 'Ha Noi', '1998-03-15', 'dungvnhe181036@fpt.edu.vn', '0967890123', 2, '', 0),
    (N'Trần Thị Hồng', N'tranthihong', N'passwordhong', N'34 Tran Quoc Toan', '1997-02-20', 'tranthihong@gmail.com', '0978901234', 3, '', 0),
    (N'Đỗ Minh Yến', N'dominhyen', N'passwordyen', N'56 Ly Thuong Kiet', '1996-01-10', 'dominhyen@gmail.com', '0989012345', 3, '', 1)
GO
--select * from Users
--update Users set IsBanned = 0 where ID = 7

--Employee
INSERT INTO Employee(UserID,Salary,HiredDate,Status)
VALUES
	(3, 3000000, '2024-04-24', N'Đang làm việc'),
    (4, 2200000, '2024-05-01', N'Đang làm việc'),
    (7, 3200000, '2024-03-15', N'Đã nghỉ'),
    (8, 2200000, '2024-02-20', N'Đang làm việc'),
    (10, 2000000, '2024-01-10', N'Đang làm việc'),
	(11, 1500000, '2024-01-10', N'Đã nghỉ')
GO

--Customer
INSERT INTO Customer(UserID,Ranking)
VALUES
	(1,N'Vàng'),
	(5,N'Bạc'),
	(6,N'Đồng'),
	(9,N'Đồng')
GO


--Category
INSERT INTO Category(Name)
VALUES
	(N'Áo'),
	(N'Quần'),
	(N'Giày'),
	(N'Mũ')
GO

--Product
INSERT INTO Product (Name, Image, Description, ReleaseDate, CategoryID, isDeleted)
VALUES
    (N'Áo Thun Nam Trơn', '/ProductImg/aothun.jpg', N'Áo thun cotton 100%, mềm mại và thấm hút mồ hôi.', '2024-01-15', 1, 0),
    (N'Áo Sơ Mi Nam Dài Tay', '/ProductImg/aosomi.jpg', N'Áo sơ mi dài tay thanh lịch, phù hợp cho công sở.', '2024-02-10', 1, 0),
    (N'Quần Jean Nam Slimfit', '/ProductImg/quanjean.jpg', N'Quần jean nam dáng ôm, phong cách hiện đại.', '2024-03-01', 2, 0),
    (N'Quần Short Kaki', '/ProductImg/quanshort.jpg', N'Quần short kaki thoải mái, phù hợp cho mùa hè.', '2024-02-20', 2, 0),
    (N'Giày Sneakers Trắng', '/ProductImg/giaysneaker.jpg', N'Giày sneakers màu trắng, dễ phối đồ.', '2024-01-30', 3, 0),
    (N'Giày Tây Nam Da Bóng', '/ProductImg/giaytay.jpg', N'Giày tây da thật, sang trọng và đẳng cấp.', '2024-03-05', 3, 0),
    (N'Mũ Lưỡi Trai Thể Thao', '/ProductImg/muluitrai.jpg', N'Mũ lưỡi trai thời trang, phù hợp cho mọi hoạt động ngoài trời.', '2024-02-25', 4, 0),
    (N'Mũ Len Giữ Ấm', '/ProductImg/mulen.jpg', N'Mũ len ấm áp, phù hợp cho mùa đông.', '2024-01-05', 4, 0),
	(N'Áo Polo Nam', '/ProductImg/aopolo.jpg', N'Áo polo vải cá sấu, phù hợp đi chơi và đi làm.', '2024-02-15', 1, 0),
    (N'Áo Khoác Gió Nam', '/ProductImg/aokhoac.jpg', N'Áo khoác chống gió, nhẹ và dễ gấp gọn.', '2024-03-10', 1, 0),
    (N'Quần Jogger Thể Thao', '/ProductImg/quanjogger.jpg', N'Quần jogger co giãn, thoải mái khi vận động.', '2024-02-05', 2, 0),
    (N'Quần Tây Công Sở', '/ProductImg/quantay.jpg', N'Quần tây cao cấp, thích hợp cho môi trường công sở.', '2024-01-25', 2, 0),
    (N'Giày Chạy Bộ Nike', '/ProductImg/giaynike.jpg', N'Giày chạy bộ chuyên dụng, hỗ trợ bàn chân tối đa.', '2024-02-28', 3, 0),
    (N'Giày Lười Nam Da Lộn', '/ProductImg/giayluoi.jpg', N'Giày lười nam da lộn, phong cách trẻ trung.', '2024-03-15', 3, 0),
    (N'Mũ Channel', '/ProductImg/muchannel.jpg', N'Mũ bảo hiểm 3/4 che chắn tốt, an toàn khi đi đường.', '2024-02-18', 4, 0),
    (N'Mũ Bucket Unisex', '/ProductImg/mubucket.jpg', N'Mũ bucket phong cách đường phố, dễ phối đồ.', '2024-03-01', 4, 0)
GO

--ProductOption
INSERT INTO ProductOption (Size, Quantity, Price, Rating, Discount, ProductID)
VALUES
    (N'S', 50, 150000, 4.5, 0, 1),
    (N'M', 40, 150000, 4.6, 0, 1),
	(N'L', 40, 150000, 4.6, 0, 1),
	(N'S', 30, 250000, 4.7, 0, 2),
    (N'M', 30, 250000, 4.7, 0, 2),
    (N'L', 25, 250000, 4.8, 0, 2),
    (N'30', 20, 400000, 4.4, 0, 3),
    (N'32', 15, 400000, 4.3, 0, 3),
    (N'S', 35, 200000, 4.6, 0, 4),
    (N'M', 30, 200000, 4.5, 0, 4),
    (N'40', 10, 800000, 4.9, 0, 5),
    (N'42', 8, 800000, 4.9, 0, 5),
    (N'39', 12, 1200000, 4.8, 0, 6),
    (N'41', 10, 1200000, 4.8, 0, 6),
    (N'Free Size', 50, 100000, 4.7, 0, 7),
    (N'Free Size', 40, 120000, 4.6, 0, 8),
	(N'M', 60, 220000, 4.6, 0, 9),
    (N'L', 50, 220000, 4.7, 0, 9),
    (N'XL', 40, 220000, 4.8, 0, 9),
    (N'M', 30, 450000, 4.5, 0, 10),
    (N'L', 25, 450000, 4.6, 0, 10),
    (N'28', 35, 300000, 4.7, 0, 11),
    (N'30', 30, 300000, 4.6, 0, 11),
    (N'32', 25, 300000, 4.5, 0, 11),
    (N'29', 40, 500000, 4.8, 0, 12),
    (N'31', 35, 500000, 4.7, 0, 12),
    (N'40', 15, 1500000, 4.9, 0, 13),
    (N'42', 12, 1500000, 4.8, 0, 13),
    (N'39', 18, 1100000, 4.7, 0, 14),
    (N'41', 16, 1100000, 4.6, 0, 14),
    (N'Free Size', 50, 350000, 4.9, 0, 15),
    (N'Free Size', 45, 200000, 4.8, 0, 16)
GO

--Vouchers
INSERT INTO Vouchers(Code,VPercent,Quantity,StartDate,EndDate,UserID)
VALUES
	('QUATANG11',15,1, CAST(N'2024-06-12T00:00:00.000' AS DateTime), CAST(N'2025-07-12T00:00:00.000' AS DateTime),NULL),
	('QUATANG33',10,0, CAST(N'2024-06-12T00:00:00.000' AS DateTime), CAST(N'2025-12-12T00:00:00.000' AS DateTime),NULL),
	('QUATANG55',15,1, CAST(N'2024-06-12T00:00:00.000' AS DateTime), CAST(N'2024-12-12T00:00:00.000' AS DateTime),NULL),
	('QUATANG66',10,2, CAST(N'2024-06-12T00:00:00.000' AS DateTime), CAST(N'2025-12-12T00:00:00.000' AS DateTime),NULL),
	(N'SUNSTORE', 30, 4, CAST(N'2024-07-17T00:00:00.000' AS DateTime), CAST(N'2025-07-26T00:00:00.000' AS DateTime),NULL),
	(N'GIFT88', 25, 2, CAST(N'2024-07-17T00:00:00.000' AS DateTime), CAST(N'2025-07-17T00:00:00.000' AS DateTime),NULL)
GO

--Orders
INSERT INTO Orders (ShipperID, VoucherID, DateTime, AdrDelivery, PhoneNumber, Note, TotalPrice, Payment, Status, DenyReason)
VALUES
    (4, 1, '2025-03-10 10:30:00', N'123 Nguyễn Trãi, Hà Nội', '0987654321', N'Giao giờ hành chính', 550000, 'COD', N'Đã đặt hàng', NULL),
    (3, NULL, '2025-03-11 15:00:00', N'456 Lê Lợi, TP. Hồ Chí Minh', '0912345678', N'Gọi trước khi giao', 1400000, 'COD', N'Đang giao hàng', NULL),
    (3, 2, '2025-03-12 09:15:00', N'789 Trần Hưng Đạo, Đà Nẵng', '0978563412', N'Giao nhanh trong ngày', 700000, 'COD', N'Đã giao hàng', NULL),
    (7, NULL, '2025-03-13 18:45:00', N'321 Phan Chu Trinh, Hải Phòng', '0962145789', N'Không giao vào buổi sáng', 1000000, 'VNP', N'Bị từ chối', N'Shop hiện tại không thể giao hàng, mong quý khách thông cảm.'),
    (3, 3, '2025-03-14 14:20:00', N'654 Bạch Đằng, Cần Thơ', '0932154876', N'Giao hàng cẩn thận', 370000, 'VNP', N'Đang giao hàng', NULL)
GO
SET IDENTITY_INSERT Orders ON;
INSERT INTO Orders (ID, DateTime) VALUES
(0,'2000-01-01')
GO
SET IDENTITY_INSERT Orders OFF;
--delete from Orders where id = 
--select * from Orders

--OrderItem
INSERT INTO OrderItem (ProductID, CustomerID, OrderID, Quantity, Price)
VALUES
    (1, 1, 1, 2, 150000),  
    (5, 1, 1, 1, 250000),  
    (9, 5, 2, 1, 200000),  
    (13, 5, 2, 1, 1200000), 
    (3, 1, 3, 2, 150000),  
    (7, 1, 3, 1, 400000), 
    (10, 6, 4, 1, 200000),
    (12, 6, 4, 1, 800000), 
    (6, 5, 5, 1, 250000),
    (16, 5, 5, 1, 120000) 
GO

--select * from ProductOption
--select * from OrderItem
--select * from Orders
--select * from Vouchers
--select * from Users
--select * from Customer