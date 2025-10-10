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

resource "aws_lb_target_group" "grafana" {
  name        = "${var.name}-grafana"
  port        = var.grafana_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/"
    protocol            = "HTTP"
    matcher             = "200-399"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
  
  tags = merge(
    var.tags,
    { "Name" = "${var.name}-grafana" }
  )
}

resource "aws_lb_target_group" "prometheus" {
  name        = "${var.name}-prometheus"
  port        = var.prometheus_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"

  health_check {
    path                = "/metrics"
    protocol            = "HTTP"
    matcher             = "200-399"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }

  tags = merge(
    var.tags,
    { "Name" = "${var.name}-prometheus" }
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

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.this.arn
<<<<<<< HEAD
  port              = 443
  protocol          = "HTTPS"
  certificate_arn   = var.certificate_arn
=======
  port              = var.web_ui_port
  protocol          = "HTTPS"
>>>>>>> 2e2e30c (chore: Add ssl)

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

resource "aws_lb_listener_rule" "https_grafana_host" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 400

  condition {
    host_header {
      values = ["grafana.${var.domain_name}"] # Match hostname
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.grafana.arn
  }
}

resource "aws_lb_listener_rule" "https_prometheus_host" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 500

  condition {
    host_header {
      values = ["prometheus.${var.domain_name}"] # Match hostname
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.prometheus.arn
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

resource "aws_lb_listener_rule" "https_anniversary_path" {  
  listener_arn = aws_lb_listener.https.arn
  priority     = 600

  condition {
    host_header {
      values = ["anniversary.${var.domain_name}"] # Match hostname
    }
  }

  action {
    type = "fixed-response"
    fixed_response {
      content_type = "text/html"
      message_body = "<!DOCTYPE html><html><head><title>Happy Anniversary!</title></head><body style=\"background-color: #f2f4f7; text-align: center; font-family: Arial, sans-serif; padding: 50px;\"><div style=\"border: 3px solid #2368a0; border-radius: 15px; padding: 20px; display: inline-block; background-color: #ffffff;\"><h1 style=\"color: #2368a0;\">&#x1F389; Happy EPAM Ukraine 20 Anniversary! &#x1F389;</h1><p style=\"font-size: 18px; color: #555;\">Celebrating 20 incredible years of<br>innovation, excellence, and teamwork!</p><footer style=\"margin-top: 20px; font-size: 12px; color: #2368a0;\">#WeAreEPAM | #20YearsTogether</footer></div></body></html>"
      status_code = "200"
    }
  }
}