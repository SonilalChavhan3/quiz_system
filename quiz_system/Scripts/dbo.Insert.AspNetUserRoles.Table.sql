SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
GO
--==============================================================================================
-- All rights reserved.
-- Author : Sonilal Chavhan
-- Description : Add role to admin user as SuperAdmin
-- Created Date : 12/26/2024
--==============================================================================================

Go
if NOT EXISTS (SELECT 1 FROM [QuizDb].[dbo].[AspNetUserRoles] where UserId='155a39b8-f44d-4e9d-8a60-3af8c3453e1d'AND RoleId='3c2e38ac-373f-4519-b648-43c21c518b21')
BEGIN
		INSERT INTO [QuizDb].[dbo].[AspNetUserRoles] (UserId, RoleId)
		VALUES ('155a39b8-f44d-4e9d-8a60-3af8c3453e1d','3c2e38ac-373f-4519-b648-43c21c518b21');
END
GO
