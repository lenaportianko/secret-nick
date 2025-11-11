terraform {
  backend "s3" {
    bucket       = "secret-nick-terraform-state"
    key          = "secret-nick-terraform.state"
    region       = "eu-central-1"
    use_lockfile = true
    encrypt      = true
  }
}
