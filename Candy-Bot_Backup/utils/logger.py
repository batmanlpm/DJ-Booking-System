"""logging configuration and utilities for the bot."""
import logging
import os
import sys
from typing import Optional

def setup_logger(name: str, log_level: Optional[str] = None) -> logging.Logger:
    """
    set up a logger with the given name and log level.
    
    args:
        name: name of the logger
        log_level: logging level (debug, info, warning, error, critical)
        
    returns:
        configured logger instance
    """
    # create logger
    logger = logging.getLogger(name)
    
    # set log level
    level_map = {
        'debug': logging.DEBUG,
        'info': logging.INFO,
        'warning': logging.WARNING,
        'error': logging.ERROR,
        'critical': logging.CRITICAL
    }
    
    # default to info if not specified or invalid
    level = level_map.get((log_level or '').lower(), logging.INFO)
    logger.setLevel(level)
    
    # prevent duplicate handlers
    if logger.handlers:
        return logger
        
    # create console handler
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setLevel(level)
    
    # create formatter
    formatter = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    console_handler.setFormatter(formatter)
    
    # add handler to logger
    logger.addHandler(console_handler)
    
    # prevent logging from propagating to the root logger
    logger.propagate = False
    
    return logger

def get_logger(name: str) -> logging.Logger:
    """
    get a logger with the given name.
    
    args:
        name: name of the logger
        
    returns:
        logger instance
    """
    return setup_logger(name)
