namespace TaskPilot.Application.Common.Utility
{
    public static class Message
    {
        public const string DUPLICATE_REQUEST = "Error: It seems like you're trying to submit the same form twice.";

        public const string INVALID_LOGIN_ATTEMPT = "Error: Invalid login attempt, please retry with correct credentials.";
        public const string NO_USER_FOUND = "Error: User not found, please proceed to registration before logging in.";
        public const string USER_EXIST = "Error: User already exist, please try logging in.";
        public const string ACCESS_DENIED = "Error: Access Denied, You're not authorized to access this resources.";
        public const string NOT_FOUND = "Error: Not Found, The resources you're trying to access is no longer available.";
        public const string EMAIL_CONFIRMATION = "Error: Email Confirmation Missing, Please Confirm Your Email & Re-try.";

        public const string PERMIT_CREATION = "A new permission has been created";
        public const string PERMIT_UPDATE = "'s permission has been updated";

        public const string PRIOR_CREATION = "A new priority has been created";
        public const string PRIOR_UPDATE = "'s priority has been updated";
        public const string PRIOR_DELETION = " priority has been deleted successfully";
        public const string PRIOR_DELETION_FAIL = "Error: Oops! It seems that you're trying to delete a priority that is currently in use";

        public const string PROF_DETAIL_EDIT = "Your profile has been updated successfully.";
        public const string PROF_PASS_EDIT = "Your credentials has been updated successfully.";
        public const string PROF_PASS_EDIT_FAIL = "Error: Oops! It seems that you entered the invalid current password, please re-try";
        public const string PROF_CONTACT_EDIT = "Your contact detail has been updated successfully";
        public const string PROF_CONTACT_EDIT_FAIL = "Error: Oops! Unable to verify your contact, please enter the correct code";
        public const string PROF_EMAIL_EDIT = "Your email has been updated successfully";
        public const string PROF_EMAIL_EDIT_FAIL = "Error: Oops! Unable to verify your email, please enter the correct code";
        public const string PROF_USERNAME_EXIST = "Error: Oops! Unable to change username as the username already exist in the system.";

        public const string ROLE_CREATION = "A new role has been created";
        public const string ROLE_UPDATE = "'s Role has been updated";
        public const string ROLE_DELETION = " roles has been deleted successfully";
        public const string ROLE_DELETION_FAIL = "Error: Oops! It seems that you're trying to delete a role that has active user in it.";

        public const string STAT_CREATION = "A new status has been created";
        public const string STAT_UPDATE = "'s Status has been updated";
        public const string STAT_DELETION = " status has been deleted successfully";
        public const string STAT_DELETION_FAIL = "Error: Oops! It seems that you're trying to delete a status that is currently in use";

        public const string USER_CREATION = "A new user has been created successfully";
        public const string USER_CREATION_FAIL = "Error: Oops! It seems that the user you try to create already exist, please re-try or logging in as the existing user.";
        public const string USER_DELETION = " users has been deleted successfully";
        public const string USER_DELETION_FAIL = "Error: Oops! It seems that you're trying to delete a user that has active task tied on him";

        public const string TASK_CREATION = "A new task has been created successfully";
        public const string TASK_NOTIF_CREATION = "A new task has been created for you";
        public const string TASK_CREATION_RECURR = " Recurring Task's has been created successfully";
        public const string TASK_UPDATE = " has been updated";
        public const string TASK_UPDATE_FAIL = "Error: Oops! It seems that you're trying to edit a task that has dependent task that yet to complete";
        public const string TASK_DELETION = " tasks has been deleted successfully";
        public const string TASK_CLOSED = "Tasks has been closed";
        public const string TASK_DEPENDENCY = "Dependency assigned successfully";
        public const string TASK_IMPORT_FAIL = "Error: Oops! It seems that you didn't input any files.";
        public const string TASK_READ_CSV = "Data retrieved from CSV successfully";
        public const string TASK_IMPORT = " tasks imported successfully";
        public const string TASK_ALREADY_CLOSED = "Error: Oops! It seems that you're trying to close a task that has already been closed.";

        public const string COMMON_ERROR = "Error: Oops! Something went wrong, please go through the error message.";
    }
}
