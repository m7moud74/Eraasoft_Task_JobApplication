using System;

namespace InfraStructure.BackGroundJobs;

public static class EmailTemplateHelper
{
    public static string GetApplicationCancelledTemplate(string candidateName, string jobTitle, DateTime cancelledAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; background-color: #f4f7f6; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: #e74c3c; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        .badge {{ display: inline-block; padding: 4px 10px; background-color: #fee2e2; color: #b91c1c; border-radius: 4px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Application Cancelled</h2>
        </div>
        <div class='content'>
            <p>Dear <strong>{candidateName}</strong>,</p>
            <p>Your application for the position of <strong>{jobTitle}</strong> has been successfully <span class='badge'>Cancelled</span>.</p>
            <p><strong>Cancellation Date:</strong> {cancelledAt:yyyy-MM-dd HH:mm} UTC</p>
            <p>If this was not done by you or you have any questions, please contact our support team.</p>
            <p>Best regards,<br>The Hiring Team</p>
        </div>
        <div class='footer'>
            &copy; 2026 Track Application System. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }

    public static string GetApplicationStatusChangedTemplate(string candidateName, string jobTitle, string oldStatus, string newStatus, DateTime updatedAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; background-color: #f4f7f6; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        .status-box {{ background-color: #eff6ff; border-left: 4px solid #2563eb; padding: 12px 16px; margin: 20px 0; border-radius: 0 4px 4px 0; }}
        .badge {{ display: inline-block; padding: 4px 10px; background-color: #dbeafe; color: #1e40af; border-radius: 4px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Application Status Update</h2>
        </div>
        <div class='content'>
            <p>Dear <strong>{candidateName}</strong>,</p>
            <p>There is an update on your application for <strong>{jobTitle}</strong>.</p>
            <div class='status-box'>
                <p style='margin: 0;'><strong>New Status:</strong> <span class='badge'>{newStatus}</span></p>
                <p style='margin: 5px 0 0 0; font-size: 13px; color: #64748b;'>Previous status: {oldStatus}</p>
            </div>
            <p><strong>Updated Date:</strong> {updatedAt:yyyy-MM-dd HH:mm} UTC</p>
            <p>Please log in to your account dashboard to view further details.</p>
            <p>Best regards,<br>The Recruitment Team</p>
        </div>
        <div class='footer'>
            &copy; 2026 Track Application System. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }

    public static string GetCompanyApprovedTemplate(string companyName, string ownerName, DateTime approvedAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 20px; background-color: #f4f7f6; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: #059669; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        .badge {{ display: inline-block; padding: 4px 10px; background-color: #d1fae5; color: #065f46; border-radius: 4px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Company Registration Approved!</h2>
        </div>
        <div class='content'>
            <p>Dear <strong>{ownerName}</strong>,</p>
            <p>Congratulations! Your company <strong>{companyName}</strong> has been reviewed and <span class='badge'>Approved</span> by an administrator.</p>
            <p><strong>Approval Date:</strong> {approvedAt:yyyy-MM-dd HH:mm} UTC</p>
            <p>You can now log in, post jobs, and invite recruiters to your company profile.</p>
            <p>Welcome aboard!<br>Track Application Team</p>
        </div>
        <div class='footer'>
            &copy; 2026 Track Application System. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }
}
