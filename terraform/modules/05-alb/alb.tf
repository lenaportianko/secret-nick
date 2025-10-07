resource "aws_lb" "this" {
  name                       = var.name
  internal                   = false
  load_balancer_type         = "application"
  security_groups            = [var.security_group_id]
  subnets                    = var.subnet_ids
  enable_deletion_protection = false

  tags = merge(
    var.tags,
    { "Name" = var.name }
  )
}

resource "aws_lb_target_group" "backend" {
  name        = "${var.name}-backend"
  port        = var.web_backend_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/api/system/info"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }

  tags = merge(
    var.tags,
    { "Name" = "${var.name}-backend" }
  )
}

resource "aws_lb_target_group" "web_ui_react" {
  name        = "${var.name}-ui-react"
  port        = var.web_ui_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/index.html"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
 
  tags = merge(
    var.tags,
    { "Name" = "${var.name}-ui_react" }
  )
}

resource "aws_lb_target_group" "web_ui_angular" {
  name        = "${var.name}-ui-angular"
  port        = var.web_ui_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/index.html"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
 
  tags = merge(
    var.tags,
    { "Name" = "${var.name}-angular" }
  )
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.this.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      protocol = "HTTPS"
      port     = "443"
      status_code = "HTTP_301" # Permanent redirect
    }
  }
}

# resource "aws_lb_listener_certificate" "marathon" {
#   listener_arn    = aws_lb_listener.https.arn
#   certificate_arn = "arn:aws:acm:eu-central-1:978652145382:certificate/91f4ccef-8f25-4c25-a3e7-1e02d256e02d"
# }

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.this.arn
  port              = var.web_ui_port
  protocol          = "HTTPS"
  certificate_arn = "arn:aws:acm:eu-central-1:978652145382:certificate/91f4ccef-8f25-4c25-a3e7-1e02d256e02d"
  
  default_action {
    type             = "forward"
    
    forward {
      target_group {
        arn  = aws_lb_target_group.web_ui_react.arn
        weight = 50 # 50% to React
      }
      target_group {
        arn  = aws_lb_target_group.web_ui_angular.arn
        weight = 50 # 50% to Angular
      }
      stickiness {
        enabled  = true
        duration = 1800
      }
    }
  }
}

resource "aws_lb_listener_rule" "https_angular_host" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 100

  condition {
    host_header {
      values = ["angular.${var.domain_name}"]
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web_ui_angular.arn
  }
}

resource "aws_lb_listener_rule" "https_react_host" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 200

  condition {
    host_header {
      values = ["react.${var.domain_name}"]
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web_ui_react.arn
  }
}

resource "aws_lb_listener_rule" "https_backend_host" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 300

  condition {
    host_header {
      values = ["backend.${var.domain_name}"] # Match hostname
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}