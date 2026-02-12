"""
Access Logger for STB Copilot

Logs all user actions for security and audit purposes.
"""

import os
import json
from datetime import datetime


class AccessLogger:
    """Logs user access and actions."""
    
    def __init__(self, log_directory='logs'):
        """
        Initialize the access logger.
        
        Args:
            log_directory (str): Directory to store log files
        """
        self.log_directory = log_directory
        self.log_file = os.path.join(log_directory, 'access.log')
        
        # Create logs directory if it doesn't exist
        os.makedirs(log_directory, exist_ok=True)
    
    def log(self, action, user_email, details=None, ip_address=None):
        """
        Log an action.
        
        Args:
            action (str): Action type (login, logout, ask_question, etc.)
            user_email (str): Email of the user performing the action
            details (dict): Additional details about the action
            ip_address (str): IP address of the user
        """
        log_entry = {
            'timestamp': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
            'action': action,
            'user': user_email,
            'ip_address': ip_address or 'unknown',
            'details': details or {}
        }
        
        # Write to log file
        try:
            with open(self.log_file, 'a', encoding='utf-8') as f:
                f.write(json.dumps(log_entry, ensure_ascii=False) + '\n')
        except Exception as error:
            print(f"Error writing to log file: {error}")
    
    def log_login(self, user_email, success, ip_address=None):
        """Log a login attempt."""
        self.log(
            action='login',
            user_email=user_email,
            details={'success': success},
            ip_address=ip_address
        )
    
    def log_logout(self, user_email, ip_address=None):
        """Log a logout."""
        self.log(
            action='logout',
            user_email=user_email,
            ip_address=ip_address
        )
    
    def log_question(self, user_email, question, confidence, ip_address=None):
        """Log a question asked."""
        self.log(
            action='ask_question',
            user_email=user_email,
            details={
                'question': question,
                'confidence': confidence
            },
            ip_address=ip_address
        )
    
    def get_recent_logs(self, limit=100):
        """
        Get recent log entries.
        
        Args:
            limit (int): Maximum number of entries to return
        
        Returns:
            list: List of log entries
        """
        if not os.path.exists(self.log_file):
            return []
        
        try:
            with open(self.log_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            
            # Get last N lines
            recent_lines = lines[-limit:]
            
            # Parse JSON
            logs = []
            for line in recent_lines:
                try:
                    logs.append(json.loads(line))
                except json.JSONDecodeError:
                    continue
            
            return logs
        except Exception as error:
            print(f"Error reading log file: {error}")
            return []
