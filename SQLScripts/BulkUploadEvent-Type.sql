CREATE TYPE dbo.BulkUploadEvent AS TABLE
(
    EventID INT,
	StartDate DATETIME,
	EndDate DATETIME,
	StartTime VARCHAR(10),
	EndTime VARCHAR(10),
	EventTitle VARCHAR(200),
	NOTES VARCHAR(1000),
	CompanyLocationID INT
);

