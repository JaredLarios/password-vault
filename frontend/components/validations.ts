export type Errors = {
  username?: string;
  name?: string;
  lastName?: string;
  password?: string;
  confirmPassword?: string;
  api?: string;
};

export function validateUserForm(data: {
  username: string;
  name: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}): Errors {
  const errors: Errors = {};
    
    if (!data.name || !data.name.trim()) {
        errors.name = "Name is required";
    }
    if (!data.lastName || !data.lastName.trim()) {
        errors.lastName = "Name is required";
    }
    if (!data.username || !data.username.trim()) {
        errors.username = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(data.username)) {
        errors.username = "Email is invalid";
    }
    if (!data.password || !data.password.trim()) {
        errors.password = "Password is required";
    } else if (data.password.length < 6) {
        errors.password = "Password must be at least 6 characters"
    }
    if (!data.confirmPassword || !data.confirmPassword.trim()) {
        errors.confirmPassword = "Please confirm your password";
    } else if (data.password !== data.confirmPassword) {
        errors.confirmPassword = "Passwords do not match";
    }
    return errors;
}